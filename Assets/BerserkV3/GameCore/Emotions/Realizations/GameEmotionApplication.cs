using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Berserk.Shared.Data.Customisation;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Exceptions;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.Models.API;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.Controllers;
using BerserkV3.GameCore.LogicEventsProcessor;
using BerserkV3.GameCore.Network;
using BerserkV3.GameCore.Repository;
using BerserkV3.Generic.ChatWheel;
using BerserkV3.Generic.Customisation;
using BerserkV3.Generic.Emotions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameCore;
using RR.Core.DebugSystem;
using Zenject;

namespace BerserkV3.GameCore.Emotions
{
	public class GameEmotionApplication : DisposableWithCts, IInitializable
	{
		private const string PREFAB_RESOURCE_PATH = "EmotionView";
		private const int MAX_DISPLAY_EMOTIONS = 3;
		private const float AUTO_CLOSE_SECONDS = 7;
		
		private readonly IChatWheel chatWheel;
		private readonly IGameRepository gameRepository;
		private readonly IGameLocks gameLocks;
		private readonly IEmotionsViewFactory viewFactory;
		private readonly IEmotionsFactory emotionsFactory;
		private readonly IGameLogicEventsSource logicEventsSource;
		private readonly IGameLogicEventsProcessor eventsProcessor;
		private readonly IGameContext gameContext;
		private readonly ICustomisationItemRepository customisationRepository;
		private readonly IInvalidActionController invalidActionController;

		private Dictionary<string, IEmotionsFieldView> fieldViews;
		private CancellationTokenSource chatWheelOpen;
		private bool initialized;
		private Tween autoClose;

		public GameEmotionApplication(
			IGameContext gameContext,
			IEnumerable<IEmotionsFieldView> fieldViews,
			ICustomisationItemRepository customisationRepository,
			IInvalidActionController invalidActionController,
			IGameLogicEventsProcessor eventsProcessor,
			IGameLogicEventsSource logicEventsSource,
			IEmotionsFactory emotionsFactory,
			IEmotionsViewFactory viewFactory, 
			IGameRepository gameRepository,
			IGameLocks gameLocks,
			IChatWheel chatWheel)
		{
			this.gameContext = gameContext;
			this.customisationRepository = customisationRepository;
			this.invalidActionController = invalidActionController;
			this.eventsProcessor = eventsProcessor;
			this.logicEventsSource = logicEventsSource;
			this.emotionsFactory = emotionsFactory;
			this.gameRepository = gameRepository;
			this.gameLocks = gameLocks;
			this.chatWheel = chatWheel;
			this.viewFactory = viewFactory;
			this.fieldViews = fieldViews.ToDictionary(x => x.FieldOwner.ToString());
		}

		public void Initialize()
		{
			logicEventsSource.Subscribe<PlayEmotion>(PlayEmotionAsync, Token);
			logicEventsSource.Subscribe<InitializeGame>(InitializeGame);
			GameCoreBus.OnRequestedChatWheel.SubscribeRaw(OnChatWheelReqested);
			CloseChatWheel();
		}

		public override void Dispose()
		{
			base.Dispose();
			autoClose?.Kill();
			autoClose = null;
			chatWheelOpen?.Cancel();
			chatWheelOpen?.Dispose();
			chatWheelOpen = null;
			fieldViews?.Clear();
			GameCoreBus.OnRequestedChatWheel.Unsubscribe(OnChatWheelReqested);
		}

		private void InitializeGame()
		{
			if (initialized)
				return;
			
			initialized = true;
			fieldViews = fieldViews.Values
				.ToDictionary(x => gameRepository.GetUserIdByOwner(x.FieldOwner));
		}

		private async UniTask PlayEmotionAsync(PlayEmotion data)
		{
			if (!CanDisplayEmotion(data.SenderId))
				return;
			
			var fieldView = fieldViews[data.SenderId];
			var view = viewFactory.Create(PREFAB_RESOURCE_PATH, fieldView.Container);
			var emotion = emotionsFactory.Create(data.EmotionId);
			await view.InitAsync(emotion);
			fieldView.Rearrange();
		}

		private async UniTask SendEmotionAsync(string id)
		{
			if (gameLocks.ByGlobal || !CanDisplayEmotion(gameRepository.SelfId) || string.IsNullOrEmpty(id))
				return;
			
			// predict emotion
			eventsProcessor.ProcessUnQueueAsync(new PlayEmotion(gameRepository.SelfId, id));
			// sent emotion to opposite user
			var emotionModel = new PlayEmotionModel
			{
				Id = id,
				RecevierUserId = gameRepository.OpponentId,
			};
			await GameAPI.PostEmotionAsync(emotionModel);
		}

		private bool CanDisplayEmotion(string userId)
		{
			return initialized 
			       && !string.IsNullOrEmpty(userId)
				   && fieldViews.TryGetValue(userId, out var fieldView)
			       && (gameRepository.SelfId != userId || fieldView.Count < MAX_DISPLAY_EMOTIONS);
		}

		private async UniTask<IChatWheelElement[]> GetChatWheelEmotionsAsync(CancellationToken token)
		{
			var customisationItems = customisationRepository.GetEquipped(CustomisationType.Emotions).ToArray();
			if (customisationItems.Length == 0)
			{
				await UniTask.Yield(); // avoid same time request to show and close view
				throw new InvalidActionException(InvalidAction.EmotionsNotCustomised);
			}
			
			var emotionViews = customisationItems
				.Select(_ =>
				{
					var view = viewFactory.Create(PREFAB_RESOURCE_PATH, chatWheel.Container);
					view.AutoDispose = false;
					return view;
				})
				.ToArray();

			await UniTask
				.WhenAll(emotionViews.Select((x, i) => x.InitAsync(emotionsFactory.Create(customisationItems[i].Id), token)))
				.AttachExternalCancellation(token);
			
			return emotionViews.OfType<IChatWheelElement>().ToArray();
		}

		private void OnChatWheelReqested(bool show)
		{
			if (!initialized)
				return;
			
			if (!show)
			{
				CloseChatWheel();
				return;
			}
			
			if (chatWheelOpen != null)
				return;
			
			autoClose?.Kill();
			autoClose = null;
			chatWheelOpen?.Cancel();
			chatWheelOpen?.Dispose();
			chatWheelOpen = new CancellationTokenSource();
			
			chatWheel.OnSlotClick += slot => SendEmotionAsync(slot?.Element?.Id).Forget();
			chatWheel.OnSlotClick += _ => RequestToCloseChatWheel();
			chatWheel.OnFocus += focus =>
			{
				if(!focus)
					RequestToCloseChatWheel();
			};

			GetChatWheelEmotionsAsync(chatWheelOpen.Token)
				.ContinueWith(elements =>
				{
					chatWheel.Init(elements.Length);
					chatWheel.Set(elements);
					chatWheel.Show();
					autoClose = DOVirtual.DelayedCall(AUTO_CLOSE_SECONDS, RequestToCloseChatWheel);
				}).Forget(e =>
				{
					if (e is InvalidActionException invalidActionException)
						invalidActionController.OnInvalidAction(invalidActionException.Value);
					
					RRLogger.Error(e);
					RequestToCloseChatWheel();
				});
		}

		private void RequestToCloseChatWheel()
		{
			GameCoreBus.OnRequestedChatWheel.Publish(false);
		}

		private void CloseChatWheel()
		{
			try
			{
				autoClose?.Kill();
				autoClose = null;
				chatWheelOpen?.Cancel();
				chatWheelOpen?.Dispose();
				chatWheelOpen = null;
				chatWheel.Close(onClosed: () => chatWheel.Clear());
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}
	}
}