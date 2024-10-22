using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.AudioSystem;
using BerserkV3.Common.AudioSystem.Abstractions;
using BerserkV3.Common.InputSystem.DragDropSystem;
using BerserkV3.Common.InputSystem.HoveringSystem;
using BerserkV3.Common.InputSystem.SelectionSystem;
using BerserkV3.Common.PreviewSystem;
using BerserkV3.GameCore.EffectsVisual.Abstractions;
using BerserkV3.GameCore.TargetSystem.Abstraction;
using DG.Tweening;
using UnityEngine;
using IDropHandler = BerserkV3.Common.InputSystem.DragDropSystem.IDropHandler;

namespace BerserkV3.GameCore.Cards
{
	public interface ICardHandStrategy : ICardStrategy {}

	public class CardHandStrategy : BaseStrategy, ICardHandStrategy, IDraggable, IHoverable, IPreviewable, ISelectable
	{
		private readonly IAnimatorApplication animatorApplication;
		private readonly IAudioApplication audioApplication;
		private readonly IGameContext gameContext;
		private readonly IHoveringSystem hoveringSystem;
		private readonly IDragDropSystem dragDropSystem;
		private readonly IPreviewSystem previewSystem;
		private readonly ISelectionSystem selectionSystem;
		private readonly IManualArrowSystem manualArrowSystem;

		private IRuntimeData RuntimeData => View.RuntimeData;

		private bool CanPlay => View is {IsSelf: true, IsLocked: false}
		                        && !manualArrowSystem.IsActive
		                        && !dragDropSystem.AnyDragged
		                        && gameContext.Timer.RuntimeData.OwnerId == RuntimeData.OwnerUserId
		                        && gameContext.PlayerRepository
			                        .Get(RuntimeData.OwnerUserId).RuntimeData.Mana >= RuntimeData.Mana;

		private static float CardSize => 1f;
		private static float OpponentCardSize => 0.9f;

		public override ICardView View { get; set; }

		public CardHandStrategy(
			ICardView cardView,
			IAnimatorApplication animatorApplication,
			IAudioApplication audioApplication,
			IGameContext gameContext,
			IHoveringSystem hoveringSystem,
			IDragDropSystem dragDropSystem,
			IPreviewSystem previewSystem,
			ISelectionSystem selectionSystem,
			IManualArrowSystem manualArrowSystem)
		{
			View = cardView;
			this.animatorApplication = animatorApplication;
			this.audioApplication = audioApplication;
			this.gameContext = gameContext;
			this.hoveringSystem = hoveringSystem;
			this.dragDropSystem = dragDropSystem;
			this.previewSystem = previewSystem;
			this.selectionSystem = selectionSystem;
			this.manualArrowSystem = manualArrowSystem;

			PreviewSettings = new PreviewSettings(PreviewType.Hand);
			HoverableSetting = HoveringSettings.Default();
			HoverableSetting.ChangeSublingIndex = false;
			DragSettings = Common.InputSystem.DragDropSystem.DragSettings.Default();
		}

		protected override void OnRefreshed()
		{
			View.GlowView.Enable(CanPlay, GlowType.Turn);
			SetTitleTextActive(View.MarkedAsSelected);
		}

		protected override void OnAllowPreviewChanged()
		{
			switch (IsPreviewAllowed)
			{
				case true:
					previewSystem.Registration(this);
					break;
				case false:
					previewSystem.UnRegistration(this);
					break;
			}
		}

		protected override void OnAllowHoverChanged()
		{
			switch (IsHoverAllowed)
			{
				case true:
					HoverableSetting.DefaultSize = View.Layout.DefaultScale;
					hoveringSystem.Registration(this);
					break;
				case false:
					hoveringSystem.UnRegistration(this);
					break;
			}
		}

		protected override void OnAllowSelectionChanged()
		{
			switch (IsSelectionAllowed)
			{
				case true:
					selectionSystem.OnSelected += OnSelected;
					selectionSystem.Registration(this);
					break;
				case false:
					selectionSystem.OnSelected -= OnSelected;
					selectionSystem.UnRegistration(this);
					View.MarkAsSelected(false);
					SetTitleTextActive(View.MarkedAsSelected);
					break;
			}
		}

		protected override void OnAllowDragChanged()
		{
			switch (IsDragAllowed)
			{
				case true:
					dragDropSystem.OnDragStart += OnDragStarted;
					dragDropSystem.OnDragCanceled += OnDragCanceled;
					dragDropSystem.Registration(this);
					break;
				case false:
					dragDropSystem.OnDragStart -= OnDragStarted;
					dragDropSystem.OnDragCanceled -= OnDragCanceled;
					dragDropSystem.UnRegistration(this);
					break;
			}
		}

		protected override void OnEnabled()
		{
			if (View?.Layout == null)
				return;

			View.SetSize(View.IsSelf ? CardSize : OpponentCardSize);
			View.Layout.SetAlpha(1f);
			View.Layout.SetInteractable(true);
			View.SelfContainer.SetAnchorsInCenter();
			
			AllowPreview(true);
			AllowHover(true);
			AllowDrag(true);
			Refresh();
		}

		#region Selectable

		GameObject ISelectable.TargetView => View?.SelfContainer.gameObject;

		public bool CanSelect()
		{
			return View is {IsSelf: true, IsLocked: false}
			       && !dragDropSystem.AnyDragged;
		}

		private void OnSelected(ISelectable selectable)
		{
			if (IsAllowedExternal || !IsSelectionAllowed || dragDropSystem.AnyDragged || selectable != this)
				return;

			View.MarkAsSelected(!View.MarkedAsSelected);
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

		public IHoverableSetting HoverableSetting { get; }

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

		public bool CanHover()
		{
			return !dragDropSystem.AnyDragged && !manualArrowSystem.IsActive;
		}

		#endregion

		#region Draggable

		GameObject IDraggable.TargetView => View?.SelfContainer.gameObject;

		public IDragSettings DragSettings { get; }

		public bool CanDrag()
		{
			return CanPlay;
		}

		public bool CanDropIn(IDropHandler place)
		{
			return IsDragAllowed;
		}

		private void OnDragStarted(IDraggable draggable)
		{
			if (draggable != this)
				return;

			var cardTransform = View.SelfContainer;
			cardTransform.DOKill();
			cardTransform.localRotation = Quaternion.identity;
			audioApplication.PlaySound(Clip.Card_StartDrag);
		}

		private void OnDragCanceled(IDraggable draggable, IDropHandler dropHandler)
		{
			if (!View?.SelfContainer || draggable != this)
				return;

			animatorApplication.EnqueueRestorePosition(View.TargetTransform, View.SelfContainer);
			audioApplication.PlaySound(View.RuntimeGameObject.Data.Type == ObjectType.Spell
				? Clip.Spell_Release
				: Clip.Card_Release);
		}

		#endregion

		#region Previewable

		GameObject IPreviewable.TargetView => View?.SelfContainer.gameObject;

		public IPreviewData PreviewData => View?.RuntimeGameObject?.ToPreviewData();

		public IPreviewSetting PreviewSettings { get; }

		public bool CanPreview()
		{
			return !manualArrowSystem.IsActive 
			       && !dragDropSystem.AnyDragged
			       && PreviewData != null;
		}

		#endregion
	}
}