using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Commands.Cmd;
using Berserk.Shared.GameCore.EffectSystem.TargetSystem;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.Models;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.InputSystem.DragDropSystem;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.Cards;
using BerserkV3.GameCore.EffectsVisual.Abstractions;
using BerserkV3.GameCore.LogicEventsProcessor;
using BerserkV3.GameCore.Network.Abstraction;
using BerserkV3.GameCore.Repository;
using BerserkV3.GameCore.TargetSystem;
using BerserkV3.GameCore.TargetSystem.Abstraction;
using BerserkV3.GameCore.UI;
using BerserkV3.Generic.UndoSystem;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameCore;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using UnityEngine;
using Zenject;
using IDropHandler = BerserkV3.Common.InputSystem.DragDropSystem.IDropHandler;

namespace BerserkV3.GameCore.Controllers
{
	public class TableController : DisposableWithCts, IInitializable, IDropHandler
	{
		private readonly IGameRepository gameRepository;
		private readonly IDragDropSystem dragDropSystem;
		private readonly ITableRaycastPanel tableRaycastPanel;
		private readonly ICardsStateMachine cardsStateContoller;
		private readonly IManualArrowSystem manualArrowSystem;
		private readonly IGameLogicEventsProcessor gameLogicEventsProcessor;
		private readonly IGameLogicEventsSource gameLogicEventsSource;
		private readonly IInvalidActionController invalidActionController;
		private readonly ISessionProcessor sessionProcessor;
		private readonly ITableCardsPositioning tableCardsPositioning;
		private readonly IAnimatorApplication animatorApplication;
		private readonly IGameDatabase gameDatabase;
		private readonly IGameContext gameContext;
		private readonly IUndoSystem undoSystem;
		private readonly IGameView gameView;
		private readonly IGameHub gameHub;
		private readonly ICardView cardMock;

		private ITargetConditionRepository TargetConditionRepository => sessionProcessor.LogicContext.TargetConditionRepository;
		private CancellationTokenSource rearrangeTableToken;

		private int? lastRelativePosition;
		private bool tableShuffled;

		public TableController(
			IGameRepository gameRepository,
			IDragDropSystem dragDropSystem,
			ITableRaycastPanel tableRaycastPanel,
			ICardsStateMachine cardsStateContoller,
			ITableCardsPositioning tableCardsPositioning,
			IManualArrowSystem manualArrowSystem,
			IGameLogicEventsProcessor gameLogicEventsProcessor,
			IGameLogicEventsSource gameLogicEventsSource,
			IInvalidActionController invalidActionController,
			ISessionProcessor sessionProcessor,
			IAnimatorApplication animatorApplication,
			IGameDatabase gameDatabase,
			IGameContext gameContext,
			IUndoSystem undoSystem,
			IGameView gameView,
			IGameHub gameHub)
		{
			this.gameRepository = gameRepository;
			this.dragDropSystem = dragDropSystem;
			this.tableRaycastPanel = tableRaycastPanel;
			this.cardsStateContoller = cardsStateContoller;
			this.tableCardsPositioning = tableCardsPositioning;
			this.manualArrowSystem = manualArrowSystem;
			this.gameLogicEventsProcessor = gameLogicEventsProcessor;
			this.gameLogicEventsSource = gameLogicEventsSource;
			this.invalidActionController = invalidActionController;
			this.sessionProcessor = sessionProcessor;
			this.animatorApplication = animatorApplication;
			this.gameContext = gameContext;
			this.undoSystem = undoSystem;
			this.gameDatabase = gameDatabase;
			this.gameView = gameView;
			this.gameHub = gameHub;
			cardMock = new CardViewMock();
		}

		public void Initialize()
		{
			GameCoreBus.OnLocalTurnPassed.SubscribeRaw(CancelTableActions);
			gameLogicEventsSource.Subscribe<TurnGame>(_ => dragDropSystem.Cancel() , Token);
			dragDropSystem.OnDragCanceled += OnDragCanceled;
			dragDropSystem.OnDroppedIn += OnDroppedIn;
			dragDropSystem.OnDragOut += OnDragOut;
			dragDropSystem.OnDrag += OnDrag;
			dragDropSystem.Registration(this);
		}

		public override void Dispose()
		{
			base.Dispose();
			rearrangeTableToken?.Cancel();
			rearrangeTableToken?.Dispose();
			rearrangeTableToken = null;
			
			GameCoreBus.OnLocalTurnPassed.Unsubscribe(CancelTableActions);
			dragDropSystem.OnDragCanceled -= OnDragCanceled;
			dragDropSystem.OnDroppedIn -= OnDroppedIn;
			dragDropSystem.OnDragOut -= OnDragOut;
			dragDropSystem.OnDrag -= OnDrag;
	
		}
		
		private int GetVirtualRelativePosition(IRuntimeObjectView target, IEnumerable<IRuntimeObjectView> neighbours)
		{
			if (neighbours == null || target == null)
				return 0;
			
			var targetPosition = target.SelfContainer.position.x;
			var neighboursPositions = neighbours.Select(x => x.SelfContainer.position.x).ToArray();
			return PositioningUtils.GetVirtualRelativePosition(targetPosition, neighboursPositions);
		}
		
		private void ExpandVirtualSpaceOnTable(ICardView draggableView)
		{
			if(draggableView == null || !draggableView.RuntimeGameObject.IsTableCard())
				return;
			
			var cardsOnTableViews = GetCardsOnTableViews();
			var draggedCardRelativeX = GetVirtualRelativePosition(draggableView, cardsOnTableViews);
			
			if (draggedCardRelativeX == lastRelativePosition)
				return;
			
			tableShuffled = true;
			lastRelativePosition = draggedCardRelativeX;
			cardMock.RuntimeData.SetRelativePositionX(draggedCardRelativeX);
			cardsOnTableViews.Insert(draggedCardRelativeX, cardMock);
			
			var cardRect = draggableView.SelfContainer.rect;
			var tableOwner = gameRepository.GetOwnerByUserId(draggableView.RuntimeData.OwnerUserId);
			var positions = tableCardsPositioning.CalculatePositions(cardsOnTableViews.Count, cardRect.width, cardRect.height, tableOwner);
			var targets = cardsOnTableViews.ZipToDictionary(positions);
			rearrangeTableToken?.Cancel();
			rearrangeTableToken?.Dispose();
			rearrangeTableToken = new CancellationTokenSource();
			animatorApplication.RestorePositionsAsync(targets, rearrangeTableToken.Token, Ease.Linear).Forget();
		}
		
		private List<ICardView> GetCardsOnTableViews()
		{
			return gameRepository.CardViews
				.Where(c => c.IsSelf && c.RuntimeData.State == RuntimeState.InTable)
				.OrderBy(c => c.RuntimeData.RelativePositionX)
				.ToList();
		}
		
		private void RearrangeTable()
		{
			rearrangeTableToken?.Cancel();
			rearrangeTableToken?.Dispose();
			rearrangeTableToken = new CancellationTokenSource();
			cardsStateContoller.RearrangeTableAsync(false, Owner.Self, rearrangeTableToken.Token).Forget();
		}
		
		private void ResetTable()
		{
			if (!tableShuffled)
				return;

			tableShuffled = false;
			lastRelativePosition = null;
			RearrangeTable();
		}

		private void CancelTableActions(Owner localTimerOwner)
		{
			dragDropSystem.Cancel();
			manualArrowSystem.Cancel();
		}
		

	#region DropHandler

		public GameObject DropHandlerView => tableRaycastPanel.TargetView;
		
		public bool CanDrop(IDraggable draggable)
		{
			if (draggable?.TargetView == null || !draggable.TargetView.TryGetComponent(out ICardView cardView))
			{
				RRLogger.Error($"{nameof(TableController)} : CanDrop {draggable?.TargetView}");
				return false;
			}

			if (cardView.RuntimeGameObject.IsTableCard()
			    && TargetConditionRepository.IsFullTable(cardView.RuntimeData.OwnerUserId))
				return false;

			return cardView.RuntimeData.State == RuntimeState.InHand;
		}
		
		private void OnDrag(IDraggable draggable)
		{
			gameView.SetPlayFieldBlockRaycast(false);
			if (dragDropSystem.CurrentDropHandler != this 
				|| draggable?.TargetView == null
				|| !draggable.TargetView.TryGetComponent(out ICardView draggableView))
				return;
			
			ExpandVirtualSpaceOnTable(draggableView);
		}
		
		private void OnDragCanceled(IDraggable draggable, IDropHandler dropHandler)
		{
			gameView.SetPlayFieldBlockRaycast(true);
			if (draggable == null || !draggable.TargetView.TryGetComponent(out ICardView cardView))
				return;
			
			if (dropHandler == this 
			    && cardView.RuntimeGameObject.IsTableCard()
			    && TargetConditionRepository.IsFullTable(cardView.RuntimeData.OwnerUserId))
				invalidActionController.OnInvalidAction(InvalidAction.TableIsFull);

			ResetTable();
		}
		
		private void OnDragOut(IDraggable draggable, IDropHandler dropHandler)
		{
			if (dropHandler != this  
				|| draggable?.TargetView == null
				|| !draggable.TargetView.TryGetComponent(out ICardView _))
				return;
			
			ResetTable();
		}
		
		private async void OnDroppedIn(IDraggable draggable, IDropHandler dropHandler)
		{
			gameView.SetPlayFieldBlockRaycast(true);
			
			if (dropHandler != this)
				return;
			
			if (draggable?.TargetView == null)
			{
				RRLogger.Error($"{nameof(OnDroppedIn)} : {nameof(IDraggable)} view is missing");
				return;
			}
			
			if (!draggable.TargetView.TryGetComponent(out ICardView cardView))
			{
				RRLogger.Error($"{nameof(OnDroppedIn)} : {draggable.TargetView.name} view is not a card!");
				return;
			}

			var isTableEntity = cardView.RuntimeGameObject.IsTableCard();
			if (isTableEntity && TargetConditionRepository.IsFullTable(cardView.RuntimeData.OwnerUserId))
			{
				RRLogger.Error($"{nameof(OnDroppedIn)} : Cancelled Board is Full");
				invalidActionController.OnInvalidAction(InvalidAction.TableIsFull);
				return;
			}

			if (gameContext.Timer.RuntimeData == null || !gameContext.Timer.RuntimeData.OwnerId.Same(gameRepository.SelfId))
			{
				RRLogger.Error($"{nameof(OnDroppedIn)} : Is not your turn");
				return;
			}
			
			var tableCards = GetCardsOnTableViews();
			var originalPosition = cardView.TargetTransform;
			var originalState = cardView.RuntimeData.State;
			var localState = isTableEntity ? RuntimeState.InTable : RuntimeState.InDeck;
			var originalXPosition = cardView.RuntimeData.RelativePositionX;
			var newXPosition = lastRelativePosition ?? GetVirtualRelativePosition(cardView, tableCards);
			var runtimePlayer = gameContext.PlayerRepository.Get(cardView.RuntimeData.OwnerUserId);
			var playerMana = (int)runtimePlayer.RuntimeData.Mana;
			lastRelativePosition = null;
			
			if (invalidActionController.HandleInvalidAction(cardView.RuntimeGameObject))
			{
				RevertBack();
				return;
			}
			
			var manualEffect = gameDatabase
				.GetEffects(cardView.RuntimeGameObject.RuntimeData.ImposingEffects)
				.FirstOrDefault(e => e.TargetMod == EffectTargetMod.PlayerPicked
				                     && e.Phases.Contains(EffectPhase.AfterSpawn));

			var param = new PlayCardArgs {RelativePositionX = newXPosition};
			var model = new CmdParamsModel(gameContext.Timer.RuntimeData.TimeHash, param)
			{
				ExecutorObjectId = cardView.RuntimeData.Id
			};
			
			// predict
			RecalculatePositions(localState, newXPosition);
			gameLogicEventsProcessor.Process(new ChangeCardsState(originalState, localState, cardView.RuntimeData.Id));
			// predict
			
			if (manualEffect != default)
			{
				var allowedTargets = TargetConditionRepository.GetAllowedTargets(cardView.RuntimeGameObject, manualEffect);
				if (allowedTargets.Length > 0)
				{
					if (allowedTargets.Length == 1 && allowedTargets.Contains(cardView.RuntimeGameObject))
					{
						model.TargetObjectsIds.Add(cardView.RuntimeData.Id);
					}
					else
					{
						try
						{
							IRuntimeObjectView fromArrowTarget = !isTableEntity 
								? gameRepository.GetHeroByUserId(cardView.RuntimeData.OwnerUserId) 
								: cardView;
				
							var pickInfo = new PickInfo(fromArrowTarget, manualEffect.Id, manualEffect.MaxTargetCount);
							
							var targets = await manualArrowSystem.GetTargetsAsync(pickInfo);
							model.TargetObjectsIds.AddRange(targets.Select(x=> x.RuntimeData.Id));
						}
						catch (OperationCanceledException)
						{
							RevertBack();
							return;
						}
					}
				}
			}

			undoSystem.Add(model.CommandId, RevertBack);
			runtimePlayer.SpendLava(cardView.RuntimeData.Mana, true);  // just predict
			cardView.RuntimeData.SetStateWithoutNotify(originalState); // reset state before send command
			await gameHub.PerformCommandAsync<PlayCardCmd>(model);

			void RevertBack()
			{
				runtimePlayer.RuntimeData.Mana.SetOrRaiseMax(playerMana);
				RecalculatePositions(originalState, originalXPosition);
				gameLogicEventsProcessor.Process(new ChangeCardsState(localState, originalState, cardView.RuntimeData.Id));
			}

			void RecalculatePositions(RuntimeState state, int position)
			{
				if (!isTableEntity) 
					return;
				
				cardView.TargetTransform = originalPosition;
				cardView.RuntimeData.SetStateWithoutNotify(state);
				cardView.RuntimeData.SetRelativePositionX(position);
				sessionProcessor.LogicContext.RuntimeCardPositionController
					.RecalculatePositions(cardView.RuntimeGameObject, false);
			}
		}

	#endregion
	}
}
