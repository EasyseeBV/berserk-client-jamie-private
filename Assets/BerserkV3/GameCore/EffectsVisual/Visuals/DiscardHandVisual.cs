using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Commands.Cmd;
using Berserk.Shared.GameCore.Models;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.InputSystem.SelectionSystem;
using BerserkV3.GameCore.Cards;
using BerserkV3.GameCore.Controllers;
using BerserkV3.GameCore.EffectsVisual.Abstractions;
using BerserkV3.GameCore.EffectsVisual.Attributes;
using BerserkV3.GameCore.Network.Abstraction;
using BerserkV3.GameCore.UI;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using UnityEngine;

namespace BerserkV3.GameCore.EffectsVisual.Visuals
{
	[EffectVisual(EffectVisualKeyword.DiscardHand)]
	public class DiscardHandVisual : EffectVisual
	{
		private const float TRANSITION_DURATION = 0.35f;
		private readonly IGameHub gameHub;
		private readonly IRuntimeTimer runtimeTimer;
		private readonly ISelectionSystem selectionSystem;
		private readonly IDiscardHandView discardHandView;
		private readonly ICardsStateMachine cardsStateMachine;
		private readonly IHorizontalPositioning horizontalPositioning;
		private readonly ISelfHandCardsPositioning selfHandCardsPositioning;
		private readonly IAnimatorApplication animatorApplication;
		private readonly IGameContext gameContext;
		private Dictionary<ICardView, (bool drag, bool preview, bool selection, RuntimeState state)> localCardStates;
		private CancellationTokenSource rearrangeSelected;
		private CancellationTokenSource rearrangeHand;
		private bool localMessageSent;
		private bool localExpired;
		
		public DiscardHandVisual(
			IGameHub gameHub,
			IRuntimeTimer runtimeTimer,
			ISelectionSystem selectionSystem,
			IDiscardHandView discardHandView,
			ICardsStateMachine cardsStateMachine,
			IHorizontalPositioning horizontalPositioning,
			IAnimatorApplication animatorApplication,
			IGameContext gameContext)
		{
			this.gameHub = gameHub;
			this.runtimeTimer = runtimeTimer;
			this.selectionSystem = selectionSystem;
			this.discardHandView = discardHandView;
			this.cardsStateMachine = cardsStateMachine;
			this.horizontalPositioning = horizontalPositioning;
			this.animatorApplication = animatorApplication;
			this.gameContext = gameContext;
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
			var handCards = GetSelfHandCardViews();
			SetupInitial(handCards);
			RefreshAllCards();
			RefreshDiscardView();
			discardHandView.ShowAsync().Forget();
			RearrangeHandAsync(0f, handCards).Forget();
			return UniTask.CompletedTask;
		}
		
		public override UniTask ChangeEffectAsync()
		{
			if (localExpired)
				return UniTask.CompletedTask;
			
			RefreshAllCards();
			RefreshDiscardView();
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
			discardHandView.SetInteractable(false);
			discardHandView.SetButtonVisibility(false);
			selectionSystem.OnSelected -= OnSelected;
			selectionSystem.Cancel();
			
			var discardCards = Targets.OfType<ICardView>().ToArray();
			var handCards = GetSelfHandCardViews().Except(discardCards).ToArray();
			cardsStateMachine.SetupCardsInHand(handCards);
			cardsStateMachine.SetupCardsInDiscard(discardCards);
			RestoreLocalStates();
			await UniTask.WhenAll(discardHandView.CloseAsync(),
				cardsStateMachine.RearrangeHandCardsAsync(true, Owner.Self, handCards));
		}
		
		private void SetupInitial(params ICardView[] views)
		{
			if (localExpired || views.Length <= 0)
				return;

			MemLocalStates(views);
			foreach (var target in views)
			{
				target.Strategy.Refresh(); // last base refresh before handle as External
				target.Strategy.AllowExternal(true);
				target.Strategy.AllowSelection(true);
				target.Strategy.AllowDrag(false);
				target.Strategy.AllowPreview(false);
				SetupCardContainer(target);
			}
		}
		
		private void RefreshDiscardView()
		{
			if (localExpired)
				return;
			
			var headerText = Model.CurrentValue switch
			{
				< 0 => gameContext.GameDatabase.GetLocalization("ClientVisual_DiscardCardsX"),
				1 => gameContext.GameDatabase.GetLocalization("ClientVisual_DiscardCardsOne"),
				_ => string.Format(gameContext.GameDatabase.GetLocalization("ClientVisual_DiscardCardsAny"), Model.CurrentValue)
			};
			discardHandView.SetHeaderText(headerText);
			discardHandView.SetButtonCallback(() => PerformUserActionAsync().Forget());
			RefreshAcceptButton();
		}

		private void RefreshAcceptButton()
		{
			var selectedCount = GetSelfSelectedCardViews().Length;
			var isLimitReachedOrDefault = selectedCount == 0 || selectedCount == Model.CurrentValue;
			discardHandView.SetInteractable(!localMessageSent && isLimitReachedOrDefault);
			discardHandView.SetButtonVisibility(!localMessageSent);
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
			SetupCardContainer(cardView);
			if (select) // only when selected
				Refresh(cardView, true);
			
			RefreshAcceptButton();
		}
		
		private void RefreshAllCards()
		{
			if (localExpired)
				return;
			
			var limit = Model.CurrentValue < 0 ? int.MaxValue : Model.CurrentValue;
			var cardsSelected = GetSelfSelectedCardViews();
			var totalCards = GetSelfHandCardViews();
			if (cardsSelected.Length > limit)
			{
				cardsSelected.TakeLast(cardsSelected.Length - limit).ForEach(v => SelectOrDeselect(v, false));
				cardsSelected = GetSelfSelectedCardViews();
			}
			
			var cardsInHand = totalCards.Except(cardsSelected).ToArray();
			var isLimitReached = cardsSelected.Length >= limit;
			
			cardsInHand.ForEach(x => Refresh(x, !isLimitReached));
			cardsSelected.ForEach(x => Refresh(x, true));
			RearrangeHandAsync(TRANSITION_DURATION, cardsInHand).Forget();
			RearrangeSelectedAsync(TRANSITION_DURATION, cardsSelected).Forget();
		}
		
		private void Refresh(ICardView view, bool canSelect)
		{
			if (localExpired || view?.GlowView == null || view.Strategy is not { } strategy)
				return;
			
			view.GlowView.Enable(canSelect && !view.MarkedAsSelected, GlowType.Turn);
			if (view.Layout is IHandCardLayout handCardLayout)
				handCardLayout.SetTitleText(view.MarkedAsSelected ? gameContext.GameDatabase.GetLocalization("ClientVisual_DiscardX_Selected") : null);
			
			strategy.AllowSelection(canSelect);
		}
		
		private UniTask RearrangeSelectedAsync(float timing, ICardView[] cardViews) =>
			RearrangeAsync(timing, 25f, ref rearrangeSelected, cardViews);
		
		private UniTask RearrangeHandAsync(float duration, ICardView[] cardViews) =>
			RearrangeAsync(duration, -25f, ref rearrangeHand, cardViews);
		
		private UniTask RearrangeAsync(float duration, float space, ref CancellationTokenSource source, ICardView[] cardViews)
		{
			source?.Cancel();
			source?.Dispose();
			source = new CancellationTokenSource();
			var size = cardViews.Select(x => x.Layout.SelfContainer.rect.width).DefaultIfEmpty(1f).Average();
			var options = new GroupPositioningOptions(Vector3.zero, cardViews.Length, size)
			{
				Alignment = TextAlignment.Center,
				Space = space
			};
			
			var positions = horizontalPositioning.CalculatePositions(options);
			var targets = cardViews.ZipToDictionary(positions);
			
			return animatorApplication.RearrangeAsync(targets, rearrangeHand.Token, duration: duration);
		}
		
		private async UniTask PerformUserActionAsync()
		{
			if (localExpired || localMessageSent)
				return;
			
			discardHandView.SetInteractable(false);
			discardHandView.SetButtonVisibility(false);
			
			var param = new PerformPhaseArgs
			{
				Initiator = Executor.RuntimeData.Id,
				Phase = EffectPhase.InShowAccepted
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
		
		private ICardView[] GetSelfHandCardViews()
		{
			return GameRepository.CardViews
				.Where(x => x.IsSelf && x.RuntimeData.State is RuntimeState.InHand)
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
		
		private void SetupCardContainer(ICardView target)
		{
			if (!target?.SelfContainer)
				return;
			
			var container = target.MarkedAsSelected ? discardHandView.MiddleContainer : discardHandView.BottomContainer;
			target.SelfContainer.SetAnchorsInCenter();
			target.SelfContainer.SetParent(container, false);
			target.SelfContainer.SetSiblingIndex(target.RuntimeData.RelativePositionX);
		}
		
		private void MemLocalStates(ICardView[] views)
		{
			localCardStates ??=
				new Dictionary<ICardView, (bool drag, bool preview, bool selection, RuntimeState state)>();
			foreach (var view in views)
			{
				if (localCardStates.ContainsKey(view))
					continue;
				
				var s = view.Strategy;
				localCardStates.Add(view,
					(s.IsDragAllowed, s.IsPreviewAllowed, s.IsSelectionAllowed, view.RuntimeData.State));
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