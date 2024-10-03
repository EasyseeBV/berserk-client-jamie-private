using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Customisation;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.LogicEvents;
using BerserkV3.Common.ProgressDrawer;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.LogicEventsProcessor;
using BerserkV3.GameCore.Repository;
using BerserkV3.Generic.Customisation;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using RR.Core.DebugSystem;
using Zenject;
using CustomisationItemDto = Berserk.Shared.Data.Customisation.CustomisationItemDto;

namespace BerserkV3.GameCore.Customisations
{
	public class GameCustomisationApplication : DisposableWithCts, IGameCustomisationApplication, IInitializable
	{
		private readonly Dictionary<string, List<CustomisationItemDto>> storage;
		private readonly ICustomisationItemRepository customisationItemRepository;
		private readonly IGameLogicEventsSource logicEventsSource;
		private readonly IProgressDrawer progressDrawer;
		private readonly IGameRepository gameRepository;
		private readonly CustomisationType[] allEnumItems;
		private readonly Queue<Func<UniTask>> waitingTasks;
		private IProgress<float> updateProgress;
		private bool isReady;

		public GameCustomisationApplication(
			ICustomisationItemRepository customisationItemRepository,
			IGameLogicEventsSource logicEventsSource,
			IProgressDrawer progressDrawer,
			IGameRepository gameRepository)
		{
			this.customisationItemRepository = customisationItemRepository;
			this.logicEventsSource = logicEventsSource;
			this.progressDrawer = progressDrawer;
			this.gameRepository = gameRepository;

			allEnumItems = Enum.GetValues(typeof(CustomisationType))
				.OfType<CustomisationType>()
				.Where(ExcludeToCheckType)
				.ToArray();
			storage = new Dictionary<string, List<CustomisationItemDto>>();
			waitingTasks = new Queue<Func<UniTask>>();
		}
		
		public void Initialize()
		{
			updateProgress = progressDrawer.CreateProgress();
			logicEventsSource.Subscribe<CustomisationsChanged>(InitializeData, Token);
		}

		private void InitializeData(CustomisationsChanged data)
		{
			if (data == null)
			{
				DefaultSharedLogger.Error($"Received null {nameof(CustomisationsChanged)}");
				return;
			}
			
			Add(data.UserId, JsonConvert.DeserializeObject<CustomisationItemDto[]>(data.EquippedCustomItems));
		}

		public override void Dispose()
		{
			base.Dispose();
			storage.Clear();
			waitingTasks.Clear();
			updateProgress?.Report(1f);
			updateProgress = null;
		}

		public void SubscribeOnReady(Func<UniTask> task)
		{
			waitingTasks.Enqueue(task);
		}

		public void SubscribeOnReady(Action task)
		{
			waitingTasks.Enqueue(() =>
			{
				task?.Invoke();
				return UniTask.CompletedTask;
			});
		}

		public T Get<T>(string userId, CustomisationType type) where T : AssetData
		{
			if (!storage.ContainsKey(userId))
			{
				RRLogger.Error("Cant get data, service not initialized!");
				return default;
			}

			if (string.IsNullOrEmpty(userId))
			{
				RRLogger.Error("Cant get data, user id is Empty!");
				return default;
			}

			var itemDto = storage[userId].FirstOrDefault(x => x.Type == type);
			if (itemDto == null)
			{
				RRLogger.Error($"Cant find data for user : {userId}, with type : {type}");
				return default;
			}

			return storage[userId].FirstOrDefault(x => x.Type == type).GetCustomisationAsset<T>();
		}

		public T GetSelf<T>(CustomisationType type) where T : AssetData
		{
			return Get<T>(gameRepository.SelfId, type);
		}

		private void Add(string userId, IEnumerable<CustomisationItemDto> itemDtos)
		{
			if (isReady)
				return;

			if (string.IsNullOrEmpty(userId))
				throw new ArgumentException("UserId can't be empty");

			var items = itemDtos?.ToList() ?? new List<CustomisationItemDto>();
			storage[userId] = items;
			ReplaceMissedByDefault(userId, items);
			if (storage.Count < 2) // TODO Rework SharedConfig.PlayerLimit
				return;

			isReady = true;
			MakeProgressFromTasksAsync().Forget();
		}

		private async UniTask MakeProgressFromTasksAsync()
		{
			float maxTasks = waitingTasks.Count;
			while (waitingTasks.Count > 0 && !Token.IsCancellationRequested)
			{
				try
				{
					await (waitingTasks.Dequeue()?.Invoke() ?? UniTask.CompletedTask);
					updateProgress?.Report((maxTasks - waitingTasks.Count) / maxTasks);
				}
				catch (Exception e)
				{
					RRLogger.Error(e);
				}
			}
			
			updateProgress?.Report(1f);
			updateProgress = null;
		}

		private void ReplaceMissedByDefault(string userId, IEnumerable<CustomisationItemDto> itemDtos)
		{
			if (ValidateCustomItems(itemDtos, out var missedTypes))
				return;

			var exist = storage[userId];

			storage[userId] = missedTypes
				.SelectMany(customisationItemRepository.Get)
				.Where(x => x.IsDefault)
				.Select(x => new CustomisationItemDto {Id = x.Id, Type = x.CustomisationType})
				.Union(exist)
				.Distinct()
				.ToList();
		}

		private bool ValidateCustomItems(IEnumerable<CustomisationItemDto> items, out List<CustomisationType> missed)
		{
			missed = new List<CustomisationType>(allEnumItems);
			foreach (var customisationItemDto in items)
			{
				missed.Remove(customisationItemDto.Type);
			}

			return missed.Count == 0;
		}

		private bool ExcludeToCheckType(CustomisationType value)
		{
			return value != CustomisationType.LobbyMusic // it can be empty
			       && value != CustomisationType.Emotions // it can be empty
			       && value != CustomisationType.None; // it can be empty
		}
	}
}