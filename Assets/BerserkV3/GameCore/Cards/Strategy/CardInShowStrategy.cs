using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using BerserkV3.Common.InputSystem.HoveringSystem;
using BerserkV3.Common.InputSystem.SelectionSystem;
using UnityEngine;

namespace BerserkV3.GameCore.Cards
{

	public class CardInShowStrategy : BaseStrategy, ISelectable, IHoverable
	{
		private readonly IGameContext gameContext;
		private readonly IHoveringSystem hoveringSystem;
		private readonly ISelectionSystem selectionSystem;
		private static float CardSize => 1f;
		public override ICardView View { get; set; }
		
		public CardInShowStrategy(
			ICardView cardView,
			IGameContext gameContext,
			IHoveringSystem hoveringSystem,
			ISelectionSystem selectionSystem)
		{
			View = cardView;
			this.gameContext = gameContext;
			this.hoveringSystem = hoveringSystem;
			this.selectionSystem = selectionSystem;
			HoverableSetting = HoveringSettings.Default();
			HoverableSetting.ChangeSublingIndex = false;
		}

		protected override void OnRefreshed()
		{
			View.SetSize(CardSize);
			View.Layout.GlowView.Enable(CanSelect() && View.MarkedAsSelected, GlowType.Turn);
			SetTitleTextActive(false);
		}

		protected override void OnEnabled()
		{
			View.Layout.SetAlpha(1f);
			View.Layout.SetInteractable(true);
			AllowHover(true);
			AllowSelection(true);
			Refresh();
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

		private void SetTitleTextActive(bool value)
		{
			if (View?.Layout is not IHandCardLayout handCardLayout)
				return;
			
			handCardLayout.SetTitleText(IsSelectionAllowed && value ? "Selected" : null);
		}

		#region Selectable
		GameObject ISelectable.TargetView => View?.SelfContainer.gameObject;

		public bool CanSelect()
		{
			return View is {IsSelf: true, IsLocked: false} 
			       && gameContext.Timer.RuntimeData != null;
		}

		private void OnSelected(ISelectable selectable)
		{
			if (IsAllowedExternal || !IsSelectionAllowed || selectable != this)
				return;
			
			View.MarkAsSelected(!View.MarkedAsSelected);
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
			return View is {IsSelf: true, IsLocked: false} 
			       && gameContext.Timer.RuntimeData != null;
		}
		#endregion
	}

}