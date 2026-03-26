using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using BerserkV3.Common.InputSystem.HoveringSystem;
using BerserkV3.Common.InputSystem.SelectionSystem;
using UnityEngine;

namespace BerserkV3.GameCore.Cards
{
	public class CardChooseStrategy : BaseStrategy, IHoverable, ISelectable
	{
		private readonly IGameContext gameContext;
		private readonly IHoveringSystem hoveringSystem;
		private readonly ISelectionSystem selectionSystem;
		private IRuntimePlayer runtimePlayer;
		
		private static float CardSize => 1.5f;
		public override ICardView View { get; set; }
		
		public CardChooseStrategy(
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
		}

		protected override void OnRefreshed()
		{
			View.SetSize(CardSize);
			View.Layout.GlowView.Enable(CanSelect() && !View.MarkedAsSelected, GlowType.Turn);
			SetExchangeTextActive(View.MarkedAsSelected);
		}

		protected override void OnEnabled()
		{
			runtimePlayer = gameContext.PlayerRepository.Get(View.RuntimeData.OwnerUserId);
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
					SetExchangeTextActive(View.MarkedAsSelected);
					break;
			}
		}
		
		private void SetExchangeTextActive(bool value)
		{
			if (View?.Layout is not IHandCardLayout handCardLayout)
				return;
			
			handCardLayout.SetTitleText(IsSelectionAllowed && value ? "Exchange" : null);
		}

	#region Selectable
		GameObject ISelectable.TargetView => View?.SelfContainer.gameObject;

		public bool CanSelect()
		{
			return View is { IsSelf: true, IsLocked: false }
			       // TODO: it's not good, need to refactor: it blocks others whose will want to use this strategy.
			       && gameContext.Timer.RuntimeData is {State: TimerState.Mulligan} 
			       && !runtimePlayer.RuntimeData.IsFinishedMulligan;
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
			return View is { IsSelf: true, IsLocked: false }
			       // TODO: it's not good, need to refactor: it blocks others whose will want to use this strategy.
			       && gameContext.Timer.RuntimeData is {State: TimerState.Mulligan} 
			       && !runtimePlayer.RuntimeData.IsFinishedMulligan;
		}
	#endregion
	}
}
