using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Commands.Cmd;
using Berserk.Shared.GameCore.Models;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.InputSystem.SelectionSystem;
using BerserkV3.GameCore.Cards;
using BerserkV3.GameCore.Controllers;
using BerserkV3.GameCore.EffectsVisual.Attributes;
using BerserkV3.GameCore.Network.Abstraction;
using BerserkV3.GameCore.UI;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;

namespace BerserkV3.GameCore.EffectsVisual.Visuals
{
	[EffectVisual(EffectVisualKeyword.ResurrectX)]
	public class ResurrectXVisual : EffectVisual
	{
		private readonly IGameHub gameHub;
		private readonly IRuntimeTimer runtimeTimer;
		private readonly ISelectionSystem selectionSystem;
		private readonly IResurrectXHandView resurrectHandView;
		private readonly ICardsStateMachine cardsStateMachine;
		private readonly ISelfHandCardsPositioning selfHandCardsPositioning;
		private readonly IGameDatabase gameDatabase;
		private Dictionary<ICardView, (bool drag, bool preview, bool selection, RuntimeState state)> localCardStates;
		private bool localMessageSent;
		private bool localExpired;
		
		public ResurrectXVisual(
			IGameHub gameHub,
			IRuntimeTimer runtimeTimer,
			ISelectionSystem selectionSystem,
			IResurrectXHandView resurrectHandView,
			ICardsStateMachine cardsStateMachine,
			IGameDatabase gameDatabase)
		{
			this.gameHub = gameHub;
			this.runtimeTimer = runtimeTimer;
			this.selectionSystem = selectionSystem;
			this.resurrectHandView = resurrectHandView;
			this.cardsStateMachine = cardsStateMachine;
			this.gameDatabase = gameDatabase;
		}
		
		public override void Dispose()
		{
			localExpired = true;
			base.Dispose();
			selectionSystem.OnSelected -= OnSelected;
		}
		
		public override UniTask ApplyLongEffectAsync()
		{
			selectionSystem.Cancel();
			selectionSystem.OnSelected -= OnSelected;
			selectionSystem.OnSelected += OnSelected;
			SetupInitial(GetSelfDiscardCardViews());
			RefreshResurrectView();
			RefreshAllCards();
			resurrectHandView.ShowAsync().Forget();
			return UniTask.CompletedTask;
		}
		
		public override UniTask ChangeEffectAsync()
		{
			if (localExpired)
				return UniTask.CompletedTask;
			
			RefreshResurrectView();
			RefreshAllCards();
			return base.ChangeEffectAsync();
		}
		
		public override UniTask StartEffectAsync()
		{
			return ExpireLongEffectAsync();
		}
		
		public override async UniTask ExpireLongEffectAsync()
		{
			if (localExpired)
				return;
			
			localExpired = true;
			resurrectHandView.SetInteractable(false);
			resurrectHandView.SetButtonVisibility(false);
			selectionSystem.OnSelected -= OnSelected;
			selectionSystem.Cancel();
			
			RestoreLocalStates();
			var resurrectedCards = Targets.OfType<ICardView>().ToArray();
			var graveyardCards = GetSelfDiscardCardViews().Except(resurrectedCards).ToArray();
			cardsStateMachine.SetupCardsInDiscard(graveyardCards);
			await UniTask.WhenAll(resurrectHandView.CloseAsync());
		}
		
		private void SetupInitial(params ICardView[] views)
		{
			if (localExpired || views.Length <= 0)
				return;
			
			MemLocalStates(views);
			foreach (var target in views)
			{
				target.SelfContainer.SetAnchorsInCenter();
				target.SelfContainer.SetParent(resurrectHandView.MiddleContainer, false);
				target.SelfContainer.SetSiblingIndex(target.RuntimeData.RelativePositionX);
				target.Strategy.Refresh(); // last base refresh before handle as External
				target.Strategy.AllowExternal(true);
				target.Strategy.AllowSelection(true);
				target.Strategy.AllowDrag(false);
				target.Strategy.AllowPreview(false);
			}
		}
		
		private void RefreshResurrectView()
		{
			if (localExpired)
				return;
			
			var headerText = Model.CurrentValue switch
			{
				< 0 => gameDatabase.GetLocalization("ClientVisual_ResurrectCardsX"),
				1 => gameDatabase.GetLocalization("ClientVisual_ResurrectCardsOne"),
				_ => string.Format(gameDatabase.GetLocalization("ClientVisual_ResurrectCardsAny"), Model.CurrentValue)
			};
			
			resurrectHandView.SetHeaderText(headerText);
			resurrectHandView.SetButtonCallback(() => PerformUserActionAsync().Forget());
			resurrectHandView.SetInteractable(!localMessageSent);
			resurrectHandView.SetButtonVisibility(!localMessageSent);
		}
		
		private void OnSelected(ISelectable selectable)
		{
			if (localExpired || selectable is not ICardStrategy { View: { } selectableView })
				return;

			SelectOrDeselect(selectableView, !selectableView.MarkedAsSelected);
			RefreshAllCards();
		}
		
		private void SelectOrDeselect(ICardView cardView, bool select)
		{
			if (localExpired)
				return;
			
			cardView.MarkAsSelected(select);
			if (select) // only when selected
				Refresh(cardView, true);
		}
		
		private void RefreshAllCards()
		{			
			if (localExpired)
				return;
			
			var limit = Model.CurrentValue < 0 ? int.MaxValue : Model.CurrentValue;
			var totalCards = GetSelfDiscardCardViews();
			var cardsSelected = GetSelfSelectedCardViews();
			if (cardsSelected.Length > limit)
			{
				cardsSelected.TakeLast(cardsSelected.Length - limit).ForEach(v => SelectOrDeselect(v, false));
				cardsSelected = GetSelfSelectedCardViews();
			}
			
			var cardsInDiscard = totalCards.Except(cardsSelected).ToArray();
			var isLimitReached = cardsSelected.Length >= limit;
			
			cardsInDiscard.ForEach(x => Refresh(x, !isLimitReached));
		}
		
		private void Refresh(ICardView view, bool canSelect)
		{
			if (localExpired || view?.GlowView == null)
				return;
			
			view.GlowView.Enable(canSelect && !view.MarkedAsSelected, GlowType.Turn);
			if (view.Layout is IHandCardLayout handCardLayout)
				handCardLayout.SetTitleText(view.MarkedAsSelected ? gameDatabase.GetLocalization("ClientVisual_ResurrectX_Selected") : null);
			
			view.Strategy.AllowSelection(canSelect);
		}
		
		private async UniTask PerformUserActionAsync()
		{
			if (localExpired || localMessageSent)
				return;
			
			resurrectHandView.SetInteractable(false);
			resurrectHandView.SetButtonVisibility(false);
			
			var param = new PerformPhaseArgs
			{
				Initiator = Executor.RuntimeData.Id,
				Phase = EffectPhase.InDiscardAccepted
			};
			
			var model = new CmdParamsModel(runtimeTimer.RuntimeData.TimeHash, param)
			{
				ExecutorObjectId = Executor.RuntimeData.Id,
				TargetObjectsIds = GetSelfSelectedCardViews()
					.Select(x => x.RuntimeData.Id)
					.ToList()
			};
			
			await gameHub.PerformCommandAsync<PerformPhaseCmd>(model);
			localMessageSent = true; // it won't apply If sending is interrupted.
			selectionSystem.OnSelected -= OnSelected; // unsubscribe only when sent successful
			selectionSystem.Cancel();
		}
		
		private ICardView[] GetSelfDiscardCardViews()
		{
			return GameRepository.CardViews
				.Where(x => x.IsSelf && x.RuntimeData.State is RuntimeState.InDiscard)
				.OrderBy(x => x.RuntimeData.RelativePositionX)
				.ToArray();
		}
		
		private ICardView[] GetSelfSelectedCardViews()
		{
			return GameRepository.CardViews
				.Where(x => x.IsSelf && x.MarkedAsSelected)
				.OrderBy(x => x.RuntimeData.RelativePositionX)
				.ToArray();
		}
		
		private void MemLocalStates(ICardView[] views)
		{
			localCardStates ??= new Dictionary<ICardView, (bool, bool, bool, RuntimeState)>();
			foreach (var view in views)
			{
				if (localCardStates.ContainsKey(view))
					continue;
				
				var s = view.Strategy;
				localCardStates.Add(view, (s.IsDragAllowed, s.IsPreviewAllowed, s.IsSelectionAllowed, view.RuntimeData.State));
			}
		}
		
		private void RestoreLocalStates()
		{
			if (localCardStates == null || localCardStates.Count == 0)
				return;
			
			foreach (var (view, (allowDrag, allowPreview, allowSelection, state)) in localCardStates)
			{
				if (!view?.SelfContainer)
					continue;
				
				view.MarkAsSelected(false);
				if (view.RuntimeData.State != state)
					continue;

				view.Strategy.AllowSelection(allowSelection);
				view.Strategy.AllowDrag(allowDrag);
				view.Strategy.AllowPreview(allowPreview);
				view.Strategy.AllowExternal(false);
			}
			
			localCardStates.Clear();
			localCardStates = null;
		}
	}
}