using System;
using System.Linq;
using System.Threading.Tasks;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Customisation;
using BerserkV3.Common.SceneService;
using Events;
using RR.Core.Extensions;
using Vulcan.Audio;
using Zenject;

namespace BerserkV3.Generic.Customisation
{
	public class CustomisationApplication : ICustomisationApplication, IInitializable, IDisposable
	{
		private readonly ICustomisationItemRepository itemRepository;
		private readonly ICustomisationItemFactory itemFactory;
		private readonly IGameDatabase gameDatabase;
		private readonly ISceneService sceneService;
		
		public CustomisationApplication(
			ICustomisationItemRepository itemRepository, 
			ICustomisationItemFactory itemFactory,
			IGameDatabase gameDatabase,
			ISceneService sceneService)
		{
			this.itemRepository = itemRepository;
			this.itemFactory = itemFactory;
			this.gameDatabase = gameDatabase;
			this.sceneService = sceneService;
		}
		
		public void Initialize()
		{
			CustomisationBus.OnMusicUpdated.SubscribeRaw(MusicController.Play);
			CustomisationBus.OnMusicUpdated.SubscribeRaw(clip =>
			{
				if (sceneService.Current == Scene.Game || DataBus.AppData.Value.LobbyMusic == clip) 
					return;
				
				DataBus.AppData.Value.LobbyMusic = clip;
				DataBus.AppData.Repeat();
			});
		}

		public async Task<bool> InitAsync()
		{
			if (itemRepository.Any())
			{
				ReInit();
				UpdateMainThemeMusic(CustomisationType.LobbyMusic);
				sceneService.OnSceneLoaded -= OnSceneLoaded;
				sceneService.OnSceneLoaded += OnSceneLoaded;
				return true;
			}
			
			var itemModels = gameDatabase.AllCustomisations();
			var equipped = await CustomisationAPI.GetUserEquippedUICustomizationsIds();
			if (!equipped)
			{
				equipped.Data = Array.Empty<string>();
				return false;
			}
			
			var items = itemModels.Select(x => itemFactory.Create(x, equipped.Data.Contains(x.Id)));
			itemRepository.AddRange(items);
			sceneService.OnSceneLoaded += OnSceneLoaded;
			UpdateMainThemeMusic(CustomisationType.LobbyMusic);
			return true;
		}

		public async Task AcceptChanges()
		{
			var equippedItems = itemRepository.GetEquipped().Select(x=> x.Id).ToArray();
			await CustomisationAPI.PostEquippСustomizations(equippedItems);
			itemRepository.Save();
		}
		
		public void CancelChanges()
		{	
			itemRepository.Reset();
		}
		
		public void UnequipItemExept(string id, CustomisationType? type = null)
		{
			var itemType = type ?? itemRepository.Get().FirstOrDefault(x => x.Id.Same(id))?.CustomisationType;
			if (!itemType.HasValue)
				throw new InvalidOperationException($"Unknown {nameof(CustomisationType)} for id : {id}");
			
			itemRepository.Get(itemType.Value)
				.Where(x => !x.Id.Same(id))
				.ForEach(x => x.Unequip());
		}

		private void ReInit()
		{
			itemRepository.Get().ForEach(item => item.Clear());
		}

		public void UpdateMainThemeMusic(CustomisationType type)
		{
			var equipped = CustomisationServiceAdapter.Repository.GetFirstEquipped(type);

			if (!MusicController.TryGetClip(equipped?.PreviewURL, out var clip) 
			    || CustomisationBus.OnMusicUpdated.Value == clip)
				return;
			
			CustomisationBus.OnMusicUpdated += clip;
		}
		

		private void OnSceneLoaded(Scene current)
		{
			if (current is Scene.Lobby)
				UpdateMainThemeMusic(CustomisationType.LobbyMusic);
			
			if (current is Scene.Game)
				UpdateMainThemeMusic(CustomisationType.BattleMusic);
		}

		public void Dispose()
		{
			CustomisationBus.OnMusicUpdated.Unsubscribe(MusicController.Play);
			sceneService.OnSceneLoaded -= OnSceneLoaded;
		}
	}
}