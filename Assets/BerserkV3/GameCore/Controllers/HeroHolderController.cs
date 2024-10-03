using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Commands.Cmd;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.Models;
using BerserkV3.Common.InputSystem.DragDropSystem;
using BerserkV3.Common.InputSystem.HoveringSystem;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.LogicEventsProcessor;
using BerserkV3.GameCore.Network.Abstraction;
using BerserkV3.GameCore.Repository;
using BerserkV3.GameCore.TargetSystem;
using BerserkV3.GameCore.TargetSystem.Abstraction;
using BerserkV3.GameCore.UI;
using BerserkV3.Generic.UndoSystem;
using Cysharp.Threading.Tasks;
using GameCore;
using RR.Core.Extensions;
using Sirenix.Utilities;
using UnityEngine;

namespace BerserkV3.GameCore.Controllers
{
	public interface IHeroHolderController : IDisposable
	{
		IRuntimeHero RuntimeHero { get; }
		IRuntimeHeroData RuntimeData { get; }
		IHeroHolderView View { get; }
		void Initialize();
	}
	
	public class HeroHolderController : DisposableWithCts, IHeroHolderController, IHoverable
	{
		private readonly IGameHub gameHub;
		private readonly IUndoSystem undoSystem;
		private readonly ISharedConfig sharedConfig;
		private readonly IGameContext gameContext;
		private readonly IGameDatabase gameDatabase;
		private readonly IGameRepository gameRepository;
		private readonly IGameLogicEventsSource gameLogicEventsSource;
		private readonly IInvalidActionController invalidActionController;
		private readonly IManualArrowSystem manualArrowSystem;
		private readonly IDragDropSystem dragDropSystem;
		private readonly IHoveringSystem hoveringSystem;
		
		public IHeroHolderView View { get; }
		public IRuntimeHero RuntimeHero { get; }
		public IRuntimeHeroData RuntimeData => RuntimeHero.RuntimeData;
		private bool initialized;
		
		public HeroHolderController(
			IGameHub gameHub, 
			IUndoSystem undoSystem,
			ISharedConfig sharedConfig, 
			IGameContext gameContext, 
			IGameDatabase gameDatabase, 
			IGameRepository gameRepository, 
			IGameLogicEventsSource gameLogicEventsSource, 
			IInvalidActionController invalidActionController, 
			IManualArrowSystem manualArrowSystem, 
			IDragDropSystem dragDropSystem, 
			IHoveringSystem hoveringSystem,
			IHeroHolderView holderView,
			IRuntimeHero runtimeHero)
		{
			this.gameHub = gameHub;
			this.undoSystem = undoSystem;
			this.sharedConfig = sharedConfig;
			this.gameContext = gameContext;
			this.gameDatabase = gameDatabase;
			this.gameRepository = gameRepository;
			this.gameLogicEventsSource = gameLogicEventsSource;
			this.invalidActionController = invalidActionController;
			this.manualArrowSystem = manualArrowSystem;
			this.dragDropSystem = dragDropSystem;
			this.hoveringSystem = hoveringSystem;
			View = holderView;
			RuntimeHero = runtimeHero;

			HoverableSetting = HoveringSettings.Default();
			HoverableSetting.ChangeSublingIndex = false;
			HoverableSetting.DefaultSize = holderView.AbilityView.localScale;
			HoverableSetting.MaxSize = 1.1f;
		}
		
		public void Initialize()
		{
			if (initialized)
				return;
			
			initialized = true;
			View.Setup();
			View.OnAbilityClick += OnAbilityUse;
			View.SetIsDisconnected(true);

			var runtimePlayer = gameContext.PlayerRepository.Get(RuntimeData.OwnerUserId);
			if (runtimePlayer.UserId == gameRepository.SelfId)
			{
				gameLogicEventsSource.Subscribe<TurnGame>(_ => UpdateAbilityVisual(), Token);
				gameLogicEventsSource.Subscribe<ChangeObjectAbilityMoves>(_ => UpdateAbilityVisual(), Token);
				gameLogicEventsSource.Subscribe<StartEffect>(_ => UpdateAbilityVisual(), Token);
				gameLogicEventsSource.Subscribe<EndEffect>(_ => UpdateAbilityVisual(), Token);
			}
			
			gameLogicEventsSource.Subscribe<ChangePlayerState>(data => UpdatePlayerState(data.RuntimePlayerData), Token);
			gameLogicEventsSource.Subscribe<ChangeConnectionStatus>(UpdateConnectionStatus, Token);
			gameLogicEventsSource.Subscribe<DeleteObjectEffect>(_ => UpdateAbilityVisual(), Token);
			GameCoreBus.OnLocalTurnPassed.SubscribeRaw(OnLocalTurnPassed);
			hoveringSystem.OnHoverEnter += OnHoverEnter;
			hoveringSystem.OnHoverExit += OnHoverExit;
			hoveringSystem.Registration(this);
			
			UpdatePlayerState(runtimePlayer.RuntimeData);
			UpdateAbilityVisual();
		}

		public override void Dispose()
		{
			base.Dispose();
			if (!initialized)
				return;

			initialized = false;
			if (hoveringSystem != null)
			{
				hoveringSystem.OnHoverEnter -= OnHoverEnter;
				hoveringSystem.OnHoverExit -= OnHoverExit;
				hoveringSystem.UnRegistration(this);
			}
			
			GameCoreBus.OnLocalTurnPassed.Unsubscribe(OnLocalTurnPassed);
			
			if (View != null)
				View.OnAbilityClick -= OnAbilityUse;
			
		}

		private bool HasAbility(out bool canUse)
		{
			var availableEffectDatas = gameDatabase.GetEffects(RuntimeData.ImposingEffects);
			var sourceEffectDatas = gameDatabase.GetEffects(RuntimeHero.Data.EffectsIds);

			var isActive = sourceEffectDatas.Any(x => x.Phases.Contains(EffectPhase.AbilityButtonPress));
			canUse = !string.IsNullOrEmpty(RuntimeData.OwnerUserId)
			         && View?.AbilityView != null
			         && gameRepository.GetOwnerByUserId(RuntimeData.OwnerUserId) == Owner.Self
			         && GameCoreBus.OnLocalTurnPassed == Owner.Self
			         && RuntimeData.OwnerUserId == gameContext.Timer.RuntimeData.OwnerId
			         && RuntimeData.AbilityMoveCount > 0
			         && !RuntimeHero.HasEffectsDisable()
			         && !RuntimeHero.HasAppliedNonDisabledEffect(EffectKeyword.Stunning)
			         && availableEffectDatas.Any(x => x.Phases.Contains(EffectPhase.AbilityButtonPress));
			
			View?.SetAbiltyText(isActive ? "A" : "P");
			return RuntimeHero.Data.EffectsIds.Count > 0;
		}
		
		private void UpdatePlayerState(IRuntimePlayerData runtimePlayerData)
		{
			if (runtimePlayerData == null || RuntimeData?.OwnerUserId != runtimePlayerData.UserId)
				return;
			
			var maxCurrManaDisplay = Math.Min(99, Mathf.Max(0, runtimePlayerData.Mana));
			var maxManaDisplay = Mathf.Min(Mathf.Max(0, runtimePlayerData.Mana.TotalMax), sharedConfig.PlayerMaxMana);
			View.LavaView.Refresh(maxCurrManaDisplay, maxManaDisplay);
			View.SetUserNameText(runtimePlayerData.UserName);
			View.SetIsBot(runtimePlayerData.IsBot);
		}

		private void UpdateConnectionStatus(ChangeConnectionStatus data)
		{
			if (RuntimeData?.OwnerUserId != data.UserId
			    || !gameContext.PlayerRepository.TryGet(data.UserId, out var runtimePlayer))
				return;
			
			View.SetIsDisconnected(!runtimePlayer.RuntimeData.IsBot && !data.IsConnected);
			DefaultSharedLogger.Log($"[{GetType().Name.Orange()}] Player connection status : [{View.GetUserNameText().Blue()} | {(data.IsConnected?"Connected".Green():"Disconnected".Red())}]");
		}

		private void UpdateAbilityVisual()
		{
			if (!View?.AbilityView || RuntimeData == null)
				return;
			
			var hasAbility = HasAbility(out var canUseAbility);
			View.SetAbilityButtonActive(hasAbility);
			View.SetAbiltyButtonInteractable(hasAbility && canUseAbility);
		}
		
		private void OnLocalTurnPassed(Owner turnOwner)
		{
			UpdateAbilityVisual();
		}
		
		private async void OnAbilityUse()
		{
			if (invalidActionController.HandleInvalidAction(RuntimeHero))
				return;

			View.SetAbiltyButtonInteractable(false);
			
			if (RuntimeData.ImposingEffects.IsNullOrEmpty())
			{
				DefaultSharedLogger.Error($"Was used abilities when any effect exist on this hero : {RuntimeHero.Data.Title}");
				return;
			}
			
			var abilityIndex = 0;
			var abilityMove = (int)RuntimeData.AbilityMoveCount;
			foreach (var effectData in gameDatabase.GetEffects(RuntimeData.ImposingEffects))
			{
				var effectModel = new CmdParamsModel(gameContext.Timer.RuntimeData.TimeHash, new PerformEffectArgs {EffectDataId = effectData.Id})
				{
					ExecutorObjectId = RuntimeData.Id,
					TargetObjectsIds = new List<int> { RuntimeData.Id }
				};
				
				if (effectData.TargetMod == EffectTargetMod.PlayerPicked)
				{
					var heroView = gameRepository.GetHeroByUserId(RuntimeData.OwnerUserId);
					var pickInfo = new PickInfo(heroView, effectData.Id, effectData.MaxTargetCount);

					try
					{
						var targets = await manualArrowSystem.GetTargetsAsync(pickInfo);
						effectModel.TargetObjectsIds = new List<int>(targets.Select(t => t.RuntimeData.Id));
					}
					catch (OperationCanceledException)
					{
						View.SetAbiltyButtonInteractable(abilityIndex == 0);
						return;
					}
				}

				abilityIndex++;
				RuntimeData.AbilityMoveCount.Set(abilityMove - 1); // just predict
				undoSystem.Add(effectModel.CommandId, () => RuntimeData.MoveCount.Set(abilityMove));
				gameHub.PerformCommandAsync<PerformAbilityCmd>(effectModel).Forget(DefaultSharedLogger.Error);
			}
		}
		
	#region Hoverable
		GameObject IHoverable.TargetView => View?.AbilityView.gameObject;

		public IHoverableSetting HoverableSetting { get; }

		public int SublingIndex { get; set; }

		public Vector3 Size
		{
			get => View.AbilityView.localScale;
			set => View.AbilityView.localScale = value;
		}

		private void OnHoverEnter(IHoverable hoverable)
		{
			if (hoverable != this)
				return;
			
		}

		private void OnHoverExit(IHoverable hoverable)
		{
			if (hoverable != this)
				return;
			
		}

		public bool CanHover()
		{
			return !dragDropSystem.AnyDragged 
			       && !manualArrowSystem.IsActive 
			       && View?.AbilityView 
			       && View.AbilityView.gameObject.activeSelf;
		}
	#endregion
	}
}