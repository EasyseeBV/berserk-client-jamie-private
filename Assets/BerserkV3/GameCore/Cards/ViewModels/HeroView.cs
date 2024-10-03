using System;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Commands.Cmd;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.Models;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.InputSystem;
using BerserkV3.Common.InputSystem.DragDropSystem;
using BerserkV3.Common.InputSystem.HoveringSystem;
using BerserkV3.Common.InputSystem.SelectionSystem;
using BerserkV3.GameCore.Controllers;
using BerserkV3.GameCore.LogicEventsProcessor;
using BerserkV3.GameCore.Network.Abstraction;
using BerserkV3.GameCore.Repository;
using BerserkV3.GameCore.TargetSystem;
using BerserkV3.GameCore.TargetSystem.Abstraction;
using BerserkV3.GameCore.TooltipPopup;
using BerserkV3.GameCore.UI;
using BerserkV3.Generic.UndoSystem;
using Cysharp.Threading.Tasks;
using GameCore;
using RR.Core.Extensions;
using UnityEngine;
using Zenject;

namespace BerserkV3.GameCore.Cards
{
	public partial class HeroView : RuntimeObjectView, IHoverable, IHeroView, ISelectable
	{
		[SerializeField] protected BaseCardLayout selfLayout ;
		[SerializeField] protected BaseCardLayout opponentLayout;
		
		private IManualArrowSystem manualArrowSystem;
		private IGameLogicEventsSource gameLogicEventsSource;
		private IGameLocks gameLocks;
		private IGameRepository gameRepository;
		private IDragDropSystem dragDropSystem;
		private ISelectionSystem selectionSystem;
		private IHoveringSystem hoveringSystem;
		private IGameContext gameContext;
		private IGameLogicContext gameLogicContext;
		private IUndoSystem undoSystem;
		private IGameHub gameHub;
		private ITooltipPopupDoubleSidedController tooltipController;

		public IEffectHintsApplication EffectHintsApplication { get; private set; }
		public IRuntimeLayout Layout { get; protected set; }

		public bool IsSelf => RuntimeData.OwnerUserId == gameRepository.SelfId;
		
		public bool IsLocked { get; private set;}

		private bool IsCanAttack()
		{
			return IsSelf
			       && !IsLocked
			       && !manualArrowSystem.IsActive
			       && !dragDropSystem.AnyDragged
			       && !gameContext.RuntimeData.IsEnded
			       && RuntimeData.Attack > 0
			       && gameLogicContext.ExecutorConditionRepository.IsCanAttack(RuntimeGameObject);
		}

		[Inject]
		public void Construct(
			IGameRepository gameRepository,
			IManualArrowSystem manualArrowSystem,
			IGameLogicEventsSource gameLogicEventsSource,
			IGameLogicContext gameLogicContext,
			IUndoSystem undoSystem,
			IGameLocks gameLocks,
			IGameContext gameContext,
			IGameHub gameHub,
			IDragDropSystem dragDropSystem,
			ISelectionSystem selectionSystem,
			IHoveringSystem hoveringSystem,
			IEffectHintsApplication effectHintsApplication,
			ITooltipPopupDoubleSidedController tooltipController)
		{
			this.gameRepository = gameRepository;
			this.manualArrowSystem = manualArrowSystem;
			this.gameLogicEventsSource = gameLogicEventsSource;
			this.gameLogicContext = gameLogicContext;
			this.undoSystem = undoSystem;
			this.gameLocks = gameLocks;
			this.gameContext = gameContext;
			this.gameHub = gameHub;
			this.dragDropSystem = dragDropSystem;
			this.selectionSystem = selectionSystem;
			this.hoveringSystem = hoveringSystem;
			this.tooltipController = tooltipController;
			EffectHintsApplication = effectHintsApplication;
			HoverableSetting = HoveringSettings.Default();
			HoverableSetting.ChangeSublingIndex = false;
			HoverableSetting.UseScaleUpAnimation = false;
		}

		private void SetupLayout(params BaseCardLayout[] layouts)
		{
			foreach (var layout in layouts)
			{
				layout.IsSelf = IsSelf;
				layout.Setup(RuntimeGameObject);
				layout.Disable();
			}
			
			Layout = IsSelf ? selfLayout : opponentLayout;
			Layout.Enable();
		}

		protected override void OnSetup()
		{
			gameRepository.RegisterHeroView(this);
			SetupLayout(selfLayout, opponentLayout);
			EffectHintsApplication.Setup(RuntimeGameObject, this);
			var token = this.GetCancellationTokenOnDestroy();
			gameLogicEventsSource.Subscribe<TurnGame>(Refresh, token);
			gameLogicEventsSource.Subscribe<EndGame>(_ => SetLock(true), token);
			gameLogicEventsSource.Subscribe<ChangePlayerState>(Refresh, token);
			
			gameLogicEventsSource.Subscribe<AddObjectEffect>(Refresh, token);
			gameLogicEventsSource.Subscribe<DeleteImposingEffects>(Refresh, token);
			gameLogicEventsSource.Subscribe<DeleteObjectEffect>(Refresh, token);
			gameLogicEventsSource.Subscribe<DeleteImmuneToDamage>(Refresh, token);
			gameLogicEventsSource.Subscribe<DeleteImmuneToKeyword>(Refresh, token);
			gameLogicEventsSource.Subscribe<StartEffect>(Refresh, token);
			gameLogicEventsSource.Subscribe<EndEffect>(Refresh, token);
			
			gameLogicEventsSource.Subscribe<ChangeObjectHp>(ChangeCardStat, token);
			gameLogicEventsSource.Subscribe<ChangeObjectAttack>(ChangeCardStat, token);
			gameLogicEventsSource.Subscribe<ChangeObjectMana>(ChangeCardStat, token);
			gameLogicEventsSource.Subscribe<ChangeObjectMoves>(ChangeCardStat, token);
			
			gameLocks.OnByQueueChanged += OnGameLocksChanged;
			GameCoreBus.OnLocalTurnPassed.Subscribe(this, Refresh);
			manualArrowSystem.OnArrowActivityChanged += Refresh;
			
			dragDropSystem.InputHandler.OnStart += OnInputStartDragDetected;
			dragDropSystem.OnDragStart += _ => Refresh();
			dragDropSystem.OnDroppedIn += (_, _) => Refresh();
			dragDropSystem.OnDragCanceled += (_,_) => Refresh();
			
			hoveringSystem.OnHoverEnter += OnHoverEnter;
			hoveringSystem.OnHoverExit += OnHoverExit;
			selectionSystem.Registration(this);
			hoveringSystem.Registration(this);
			Refresh();
		}

		private void Refresh()
		{
			Layout?.GlowView?.Enable(IsCanAttack(), GlowType.Turn);
		}
		
		public void SetLock(bool value)
		{
			IsLocked = value || gameContext.RuntimeData.IsEnded;
			Refresh();
		}

		private void ChangeCardStat(ChangeStatBase data)
		{
			if (data.RuntimeObjectId == RuntimeData.Id)
				Refresh();
		}
		
		private void OnGameLocksChanged()
		{
			if (gameContext.Timer.RuntimeData.State != TimerState.Mulligan)
				SetLock(gameLocks.ByQueueProcessed);
		}
		
		private void OnDestroy()
		{
			Layout?.Disable();
			
			if(dragDropSystem?.InputHandler != null)
				dragDropSystem.InputHandler.OnStart -= OnInputStartDragDetected;
			
			if(manualArrowSystem != null)
				manualArrowSystem.OnArrowActivityChanged -= Refresh;

			if (hoveringSystem != null)
			{
				hoveringSystem.OnHoverEnter -= OnHoverEnter;
				hoveringSystem.OnHoverExit -= OnHoverExit;
				hoveringSystem.UnRegistration(this);
			}
			
			if (gameLocks != null)
				gameLocks.OnByQueueChanged -= OnGameLocksChanged;
			
			GameCoreBus.OnLocalTurnPassed.Unsubscribe(this);
			selectionSystem?.UnRegistration(this);
			EffectHintsApplication?.Dispose();
			EffectHintsApplication = null;
			manualArrowSystem = null;
			gameRepository = null;
			gameContext = null;
			gameHub = null;
		}

	#region Handle Input
		private async void OnInputStartDragDetected()
		{
			if (!IsCanAttack())
				return;
			
			if (!InputHelper.IsPointerOver(SelfContainer.gameObject))
				return;
			
			try
			{
				var moveCount = (int)RuntimeData.MoveCount;
				var manualEffect = gameContext.GameDatabase.GetEffectConfig(EffectKeyword.GenericAttack.AsSystemEffectId());
				var pickInfo = new PickInfo(this, manualEffect.Id, manualEffect.MaxTargetCount);
				var targets = await manualArrowSystem.GetTargetsAsync(pickInfo);

				var model = new CmdParamsModel(gameContext.Timer.RuntimeData.TimeHash)
				{
					ExecutorObjectId = RuntimeGameObject.RuntimeData.Id,
					TargetObjectsIds = targets.Select(t => t.RuntimeGameObject.RuntimeData.Id).ToList(),
				};

				RuntimeData.MoveCount.Set(moveCount - 1); // just predict
				undoSystem.Add(model.CommandId, () => RuntimeData.MoveCount.Set(moveCount));
				await gameHub.PerformCommandAsync<PerformAttackCmd>(model, true);
			}
			catch (OperationCanceledException)
			{
				DefaultSharedLogger.Log($"[{GetType().Name.Orange()}] Manual arrow cancelled : {RuntimeGameObject.Data.Title.Bold()}");
			}
		}
	#endregion

	#region Selectable
		GameObject ISelectable.TargetView => SelfContainer.gameObject;

		public bool CanSelect()
		{
			return manualArrowSystem.IsActive && !dragDropSystem.AnyDragged;
		}
	#endregion

	#region Hoverable
		GameObject IHoverable.TargetView => Layout.SelfContainer.gameObject;
		public IHoverableSetting HoverableSetting { get; private set; }
		public int SublingIndex { get; set; } // implement if need scale up animation
		public Vector3 Size { get; set; } // implement if need scale up animation

		public bool CanHover()
		{
			return !dragDropSystem.AnyDragged;
		}
		
		private void OnHoverExit(IHoverable hoverable)
		{
			if (Layout?.GlowView == null || hoverable != this)
				return;

			Layout.GlowView.Enable(false, GlowType.Targeting);
			tooltipController.Close();
		}

		private void OnHoverEnter(IHoverable hoverable)
		{
			if (Layout?.GlowView == null || hoverable != this)
				return;

			if (manualArrowSystem.IsActive)
			{
				var executor = manualArrowSystem.Current?.From?.RuntimeGameObject;
				var effectId = manualArrowSystem.Current?.EffectId;
				var canSelection = executor != null 
				                   && !string.IsNullOrEmpty(effectId)
				                   && gameLogicContext.TargetConditionRepository.IsAllowedTarget(executor, Layout.RuntimeGameObject, effectId);
				
				Layout.GlowView.Enable(canSelection, GlowType.Targeting);
			}
			else
			{
				tooltipController.DisplayAsync(RuntimeData, Layout.SelfContainer);
			}
		}
	#endregion
	}
}