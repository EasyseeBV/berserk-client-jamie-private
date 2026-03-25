using System;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.RuntimeObjects;
using BerserkV3.Common.InputSystem.DragDropSystem;
using BerserkV3.Common.InputSystem.HoveringSystem;
using BerserkV3.Common.InputSystem.SelectionSystem;
using BerserkV3.Common.PreviewSystem;
using BerserkV3.GameCore.TargetSystem.Abstraction;
using BerserkV3.GameCore.TooltipPopup;
using UI;
using UnityEngine;
using Zenject;

namespace BerserkV3.GameCore.Cards
{

	public class CardInTableStartegy : BaseStrategy, IHoverable, ISelectable, IPreviewable
	{
		protected IGameContext GameContext;
		protected IDragDropSystem DragDropSystem;
		protected IHoveringSystem HoveringSystem;
		protected ISelectionSystem SelectionSystem;
		protected IPreviewSystem PreviewSystem;
		protected IManualArrowSystem ManualArrowSystem;
		protected IGameLogicContext GameLogicContext;
		protected ITooltipPopupDoubleSidedController TooltipController;
		public IEffectHintsApplication EffectHintsApplication { get; private set; }
		protected virtual float CardSize => ResolveCardSize();
		public override ICardView View { get; set; }
		
		[Inject]
		public void Construct(
			ICardView cardView,
			IManualArrowSystem manualArrowSystem,
			IGameLogicContext gameLogicContext,
			IGameContext gameContext,
			IDragDropSystem dragDropSystem,
			IHoveringSystem hoveringSystem,
			ISelectionSystem selectionSystem,
			IPreviewSystem previewSystem,
			IEffectHintsApplication effectHintsApplication,
			ITooltipPopupDoubleSidedController tooltipController)
		{
			View = cardView;
			ManualArrowSystem = manualArrowSystem;
			GameLogicContext = gameLogicContext;
			GameContext = gameContext;
			DragDropSystem = dragDropSystem;
			HoveringSystem = hoveringSystem;
			SelectionSystem = selectionSystem;
			PreviewSystem = previewSystem;
			EffectHintsApplication = effectHintsApplication;
			TooltipController = tooltipController;

			HoverableSetting = HoveringSettings.Default();
			PreviewSettings = new PreviewSettings(PreviewType.Creature);
		}

		protected override void OnEnabled()
		{
			EffectHintsApplication.Setup(View.RuntimeGameObject, View);
			View.Layout.SetAlpha(1f);
			View.Layout.SetInteractable(true);
			AllowHover(true);
			AllowSelection(true);
			AllowPreview(true);
			Refresh();
		}

		protected override void OnDisabled()
		{
			base.OnDisabled();
			EffectHintsApplication.Dispose();
		}

		protected override void OnRefreshed()
		{
			View.SetSize(CardSize);
			View.Layout.Refresh();
			SetTitleTextActive(View.MarkedAsSelected);
		}

		private float ResolveCardSize()
		{
			if (!BoardLayoutSettings.IsCompact())
				return 1f;

			var ownerUserId = View?.RuntimeData?.OwnerUserId;
			if (string.IsNullOrWhiteSpace(ownerUserId) || GameContext?.GameRuntimePool == null)
				return 0.72f;

			var count = GameContext.GameRuntimePool
				.OfType<IRuntimeGameCard>()
				.Count(card =>
					card.RuntimeData.State == RuntimeState.InTable &&
					card.RuntimeData.OwnerUserId == ownerUserId &&
					ObjectType.TableCardsMask.HasFlag(card.Data.Type));

			var isMinimal = BoardLayoutSettings.IsMinimal();
			var scale = (isMinimal ? 0.90f : 0.92f) - Math.Max(0, count - 1) * (isMinimal ? 0.06f : 0.06f);
			return Mathf.Clamp(scale, isMinimal ? 0.58f : 0.58f, isMinimal ? 0.85f : 0.84f);
		}
		
		protected override void OnAllowPreviewChanged()
		{
			switch (IsPreviewAllowed)
			{
				case true:
					PreviewSystem.Registration(this);
					break;
				case false:
					PreviewSystem.UnRegistration(this);
					break;
			}
		}

		protected override void OnAllowHoverChanged()
		{
			switch (IsHoverAllowed)
			{
				case true:
					HoverableSetting.DefaultSize = View.Layout.DefaultScale;
					HoveringSystem.OnHoverEnter += OnHoverEnter;
					HoveringSystem.OnHoverExit += OnHoverExit;
					HoveringSystem.Registration(this);
					break;
				case false:
					HoveringSystem.OnHoverEnter -= OnHoverEnter;
					HoveringSystem.OnHoverExit -= OnHoverExit;
					HoveringSystem.UnRegistration(this);
					break;
			}
		}

		protected override void OnAllowSelectionChanged()
		{
			switch (IsSelectionAllowed)
			{
				case true:
					SelectionSystem.Registration(this);
					break;
				case false:
					SelectionSystem.UnRegistration(this);
					View.MarkAsSelected(false);
					SetTitleTextActive(View.MarkedAsSelected);
					break;
			}
		}

		#region Previwable
		GameObject IPreviewable.TargetView => View?.Layout?.SelfContainer.gameObject;

		public IPreviewData PreviewData => View?.RuntimeGameObject?.ToPreviewData();

		public IPreviewSetting PreviewSettings { get; private set; }

		public bool CanPreview()
		{
			return !ManualArrowSystem.IsActive && !DragDropSystem.AnyDragged;
		}
		#endregion

		#region Selectable
		GameObject ISelectable.TargetView => View?.Layout?.SelfContainer.gameObject;

		public bool CanSelect()
		{
			return View?.Layout?.GlowView != null;
		}
		
		private void SetTitleTextActive(bool value)
		{
			if (View?.Layout is not IHandCardLayout handCardLayout)
				return;
			
			handCardLayout.SetTitleText(IsSelectionAllowed && value ? "Selected" : null);
		}
		
		#endregion

		#region Hoverable
		GameObject IHoverable.TargetView => View?.Layout?.SelfContainer.gameObject;

		public IHoverableSetting HoverableSetting { get; private set; }

		public int SublingIndex
		{
			get => View.SelfContainer.GetSiblingIndex();
			set => View.SelfContainer.SetSiblingIndex(value);
		}

		public Vector3 Size
		{
			get => View.Layout.SelfContainer.localScale;
			set => View.Layout.SelfContainer.localScale = value;
		}

		private void OnHoverEnter(IHoverable hoverable)
		{
			if (IsAllowedExternal || !IsHoverAllowed || View?.Layout?.GlowView == null || hoverable != this)
				return;

			if (ManualArrowSystem.IsActive)
			{
				var executor = ManualArrowSystem.Current?.From?.RuntimeGameObject;
				var effectId = ManualArrowSystem.Current?.EffectId;
				var canSelection = executor != null 
				                   && !string.IsNullOrEmpty(effectId)
				                   && GameLogicContext.TargetConditionRepository.IsAllowedTarget(executor, View.Layout.RuntimeGameObject, effectId);
				
				View.Layout.GlowView.Enable(canSelection, GlowType.Targeting);
			}
			else
			{
				TooltipController.DisplayAsync(View.RuntimeGameObject.RuntimeData, View?.Layout?.SelfContainer);
			}
		}

		private void OnHoverExit(IHoverable hoverable)
		{
			if (IsAllowedExternal || !IsHoverAllowed || View?.Layout?.GlowView == null || hoverable != this)
				return;
			
			TooltipController.Close();
			
			View.GlowView.Enable(false, GlowType.Targeting);
		}

		public bool CanHover()
		{
			return !DragDropSystem.AnyDragged;
		}
		#endregion
	}

}
