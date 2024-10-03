using BerserkV3.Common.InputSystem.DragDropSystem;
using BerserkV3.Common.InputSystem.HoveringSystem;
using BerserkV3.Common.InputSystem.SelectionSystem;
using BerserkV3.Common.PreviewSystem;
using BerserkV3.GameCore.UI;
using UnityEngine;

namespace BerserkV3.GameCore.Cards
{
	public class CardDiscardStrategy : BaseStrategy, ICardHandStrategy, ISelectable, IHoverable, IPreviewable
	{
		private readonly IGraveyardView graveyardView;
		private readonly IDragDropSystem dragDropSystem;
		private readonly ISelectionSystem selectionSystem;
		private readonly IHoveringSystem hoveringSystem;
		private readonly IPreviewSystem previewSystem;

		private static float CardSize => 0.85f;
		public override ICardView View { get; set; }

		public CardDiscardStrategy(
			ICardView cardView,
			IGraveyardView graveyardView,
			IDragDropSystem dragDropSystem,
			ISelectionSystem selectionSystem,
			IHoveringSystem hoveringSystem,
			IPreviewSystem previewSystem)
		{
			View = cardView;
			this.graveyardView = graveyardView;
			this.dragDropSystem = dragDropSystem;
			this.selectionSystem = selectionSystem;
			this.hoveringSystem = hoveringSystem;
			this.previewSystem = previewSystem;
			HoverableSetting = HoveringSettings.Default();
			HoverableSetting.ChangeSublingIndex = false;
			PreviewSettings = new PreviewSettings(PreviewType.Graveyard);
		}

		protected override void OnRefreshed()
		{
			View.Layout.SetInteractable(true);
			View.Layout.SetAlpha(1f);
			View.SetSize(CardSize);
			View.GlowView.Disable();
			SetTitleTextActive(View.MarkedAsSelected);
		}

		protected override void OnEnabled()
		{
			AllowHover(true);
			AllowPreview(true);
			Refresh();
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

		#region Previewable

		GameObject IPreviewable.TargetView => View?.SelfContainer.gameObject;

		public IPreviewData PreviewData => View?.RuntimeGameObject?.ToPreviewData();

		public IPreviewSetting PreviewSettings { get; }

		public bool CanPreview()
		{
			return graveyardView.IsShowed && PreviewData != null;
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
			return graveyardView.IsShowed;
		}

		#endregion
	}
}