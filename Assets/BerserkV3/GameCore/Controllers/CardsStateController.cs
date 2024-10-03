using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Commands.Cmd;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.Models;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.Cards;
using BerserkV3.GameCore.EffectsVisual.Abstractions;
using BerserkV3.GameCore.EffectsVisual.Models;
using BerserkV3.GameCore.LogicEventsProcessor;
using BerserkV3.GameCore.Network.Abstraction;
using BerserkV3.GameCore.Repository;
using BerserkV3.GameCore.TargetSystem;
using BerserkV3.GameCore.TargetSystem.Abstraction;
using BerserkV3.GameCore.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace BerserkV3.GameCore.Controllers
{

	public interface ICardsStateMachine
	{
		void SetupCardsInHand(IEnumerable<ICardView> newCardsInHandViews);
		void SetupCardsOnTable(IEnumerable<ICardView> newCardsOnTableViews);
		void SetupCardsInDiscard(IEnumerable<ICardView> newCardsInDiscardViews);
		void SetupCardsInShowAll(IEnumerable<ICardView> newCardsInShowViews);

		UniTask RearrangeHandCardsAsync(bool force, Owner owner, params ICardView[] cardViews);
		UniTask RearrangeInShowAllAsync(ICardView[] targets);
		UniTask RearrangeTableAsync(bool force, Owner owner, CancellationToken? token = null);
	}

	public class CardsStateController : DisposableWithCts, IInitializable, ICardsStateMachine
	{
		private const int ROW_THRESHOLD = 7;
		private readonly IGameRepository gameRepository;
		private readonly IGameDatabase gameDatabase;
		private readonly IAnimatorApplication animatorApplication;
		private readonly IRuntimeEffectModelFatory effectModelFatory;
		private readonly ITableCardsPositioning tableCardsPositioning;
		private readonly IOpponentHandCardsPositioning opponentHandCardsPositioning;
		private readonly ISelfHandCardsPositioning selfHandCardsPositioning;
		private readonly IGameLogicEventsSource gameLogicEventsSource;
		private readonly IVisualSequenceApplication sequenceApplication;
		private readonly IManualArrowSystem manualArrowSystem;
		private readonly IGameLogicContext gameLogicContext;
		private readonly IGameContext gameContext;
		private readonly IGameContainers gameContainers;
		private readonly IGameHub gameHub;
		private CancellationTokenSource rearrangeTableSelf;
		private CancellationTokenSource rearrangeTableOpponent;

		[Inject]
		public CardsStateController(
			IGameRepository gameRepository,
			IGameDatabase gameDatabase,
			IAnimatorApplication animatorApplication,
			IRuntimeEffectModelFatory effectModelFatory,
			ITableCardsPositioning tableCardsPositioning,
			IOpponentHandCardsPositioning opponentHandCardsPositioning,
			ISelfHandCardsPositioning selfHandCardsPositioning,
			IGameLogicEventsSource gameLogicEventsSource,
			IVisualSequenceApplication sequenceApplication,
			IManualArrowSystem manualArrowSystem,
			IGameLogicContext gameLogicContext,
			IGameContext gameContext,
			IGameContainers gameContainers,
			IGameHub gameHub)
		{
			this.gameRepository = gameRepository;
			this.gameDatabase = gameDatabase;
			this.animatorApplication = animatorApplication;
			this.effectModelFatory = effectModelFatory;
			this.tableCardsPositioning = tableCardsPositioning;
			this.opponentHandCardsPositioning = opponentHandCardsPositioning;
			this.selfHandCardsPositioning = selfHandCardsPositioning;
			this.gameLogicEventsSource = gameLogicEventsSource;
			this.sequenceApplication = sequenceApplication;
			this.manualArrowSystem = manualArrowSystem;
			this.gameLogicContext = gameLogicContext;
			this.gameContext = gameContext;
			this.gameContainers = gameContainers;
			this.gameHub = gameHub;
		}

		public void Initialize()
		{
			gameLogicEventsSource.Subscribe<ChangeCardsState>(ChangeCardsStateAsync, Token);
		}

		public override void Dispose()
		{
			base.Dispose();
			rearrangeTableSelf?.Cancel();
			rearrangeTableSelf?.Dispose();
			rearrangeTableOpponent?.Cancel();
			rearrangeTableOpponent?.Dispose();
			rearrangeTableOpponent = null;
			rearrangeTableSelf = null;
		}

		private async UniTask ChangeCardsStateAsync(ChangeCardsState state)
		{
			var changedCards = state.CardIds
				.Select(gameRepository.GetCardViewByRuntimeId)
				.OrderBy(x => x.RuntimeData.RelativePositionX)
				.ToArray();

			var selfChangedCards = changedCards
				.Where(c => c.IsSelf)
				.ToArray();

			var opponentChangedCards = changedCards
				.Where(c => !c.IsSelf)
				.ToArray();

			changedCards.ForEach(x =>
			{
				if (x.RuntimeData.Type == ObjectType.Spell
				    && state.NewState == RuntimeState.InTable) // spells on table will manual resolving below
					return;
				
				x.SetLocalState(state.NewState);
			});

			//InDeck or InShow ->InTable
			if (state.OldState is RuntimeState.InDeck or RuntimeState.InShow
			    && state.NewState == RuntimeState.InTable)
			{
				var tableCards = HandleSpellsOnTable();
				SetupCardsOnTable(tableCards);
				await UniTask.WhenAll(RearrangeTableAsync(selfChangedCards, opponentChangedCards));
				await HandleManualEffectSelection(selfChangedCards);
			}
			//InDeck->InHand
			else if (state.OldState == RuntimeState.InDeck && state.NewState == RuntimeState.InHand)
			{
				SetupCardsInHand(changedCards);
				await RearrangeHandCardsAsync(selfChangedCards);
			}
			//InHand->InTable
			else if (state.OldState == RuntimeState.InHand && state.NewState == RuntimeState.InTable)
			{
				SetupCardsOnTable(HandleSpellsOnTable());
				await UniTask.WhenAll(RearrangeHandCardsAsync(selfChangedCards), 
					RearrangeTableAsync(selfChangedCards, opponentChangedCards));
			}
			//InHand->InDiscard
			else if (state.OldState == RuntimeState.InHand && state.NewState == RuntimeState.InDiscard)
			{
				//await PlayDeathAnimationAsync(changedCards);

				SetupCardsInDiscard(changedCards);

				await RearrangeHandCardsAsync(selfChangedCards);
			}
			//InHand->InDeck
			else if (state.OldState == RuntimeState.InHand && state.NewState == RuntimeState.InDeck)
			{
				SetupCardsInDeck(selfChangedCards);
				await RearrangeHandCardsAsync(selfChangedCards);
			}
			//InHand->InExile
			else if (state.OldState == RuntimeState.InHand && state.NewState == RuntimeState.InExile)
			{
				await RearrangeHandCardsAsync(selfChangedCards);
			}
			//InHand->InHand
			else if (state.OldState == RuntimeState.InHand && state.NewState == RuntimeState.InHand)
			{
				SetupCardsInHand(changedCards);
				await RearrangeHandCardsAsync(selfChangedCards);
			}
			//InTable->InDiscard
			else if (state.OldState == RuntimeState.InTable && state.NewState == RuntimeState.InDiscard)
			{
				SetupCardsInDiscard(changedCards);

				await RearrangeTableAsync(selfChangedCards, opponentChangedCards);
			}
			//InTable or InShow ->InExile
			else if (state.OldState is RuntimeState.InTable or RuntimeState.InShow
			         && state.NewState == RuntimeState.InExile)
			{
				SetupCardsInDeck(changedCards);
				await RearrangeTableAsync(selfChangedCards, opponentChangedCards);
			}
			//InTable->InHand
			else if (state.OldState == RuntimeState.InTable && state.NewState == RuntimeState.InHand)
			{
				SetupCardsInHand(changedCards);

				await UniTask.WhenAll(
					RearrangeHandCardsAsync(selfChangedCards),
					RearrangeTableAsync(selfChangedCards, opponentChangedCards));
			}
			//InChoose->InHand
			else if (state.OldState == RuntimeState.InChoose && state.NewState == RuntimeState.InHand)
			{
				SetupCardsInHand(changedCards);
				RearrangeHandCardsAsync(selfChangedCards).Forget();
			}
			//InDiscard->InTable
			else if (state.OldState == RuntimeState.InDiscard && state.NewState == RuntimeState.InTable)
			{
				SetupCardsOnTable(HandleSpellsOnTable());
				await RearrangeTableAsync(selfChangedCards, opponentChangedCards);
			}
			//InDiscard->InHand
			else if (state.OldState == RuntimeState.InDiscard && state.NewState == RuntimeState.InHand)
			{
				SetupCardsInHand(changedCards);
				await RearrangeHandCardsAsync(selfChangedCards);
			}
			//InShowAll->InDiscard
			else if (state.OldState == RuntimeState.InShowAll && state.NewState == RuntimeState.InDiscard)
			{
				SetupCardsInDiscard(changedCards);
			}
			//AnyState->InShowAll
			else if (state.NewState == RuntimeState.InShowAll)
			{
				SetupCardsInShowAll(changedCards);
				await RearrangeInShowAllAsync(changedCards);
			}

			ICardView[] HandleSpellsOnTable()
			{
				var spellOnTable = changedCards.Where(x => x.RuntimeData.Type == ObjectType.Spell).ToArray();
				if (spellOnTable.Length > 0)
				{
					spellOnTable.ForEach(x => x.SetLocalState(RuntimeState.InDiscard));
					SetupCardsInDiscard(spellOnTable);
				}
				
				return changedCards.Except(spellOnTable).ToArray();
			}
		}

		private void SetupCardsInDeck(IEnumerable<ICardView> newCardsInDeckViews)
		{
			newCardsInDeckViews?.ForEach(x =>
			{
				var parent = gameRepository.SelfId == x.RuntimeData.OwnerUserId
					? gameContainers.DeckSelfContainer
					: gameContainers.DeckOpponentContainer;
				x.SelfContainer.SetAnchorsInCenter();
				x.SelfContainer.SetParent(parent, false);
				x.SelfContainer.localPosition = Vector3.zero;
			});
		}

		public void SetupCardsInHand(IEnumerable<ICardView> newCardsInHandViews)
		{
			newCardsInHandViews?.ForEach(x =>
			{
				var parent = x.IsSelf
					? gameContainers.SelfHandContainer
					: gameContainers.OpponentHandContainer;
				x.SelfContainer.SetAnchorsInCenter();
				x.SelfContainer.SetParent(parent, false);
			});
		}
		
		public void SetupCardsInShowAll(IEnumerable<ICardView> newCardsInShowViews)
		{
			var count = 0;
			
			if (gameContainers.InShowSecondRow.gameObject.activeSelf)
				gameContainers.InShowSecondRow.gameObject.SetActive(false);
			
			newCardsInShowViews?.ForEach(x =>
			{
				if (count < ROW_THRESHOLD)
				{
					x.SelfContainer.SetAnchorsInCenter();
					x.SelfContainer.SetParent(gameContainers.InShowFirstRow, false);
				}
				else
				{
					gameContainers.InShowSecondRow.gameObject.SetActive(true);
					x.SelfContainer.SetAnchorsInCenter();
					x.SelfContainer.SetParent(gameContainers.InShowSecondRow, false);
				}
				x.SelfContainer.localRotation = Quaternion.identity;
				count++;
			});
		}

		public void SetupCardsOnTable(IEnumerable<ICardView> newCardsOnTableViews)
		{
			newCardsOnTableViews?.ForEach(x =>
			{
				x.SelfContainer.SetAnchorsInCenter();
				x.SelfContainer.SetParent(gameContainers.TableContainer, false);
				x.SelfContainer.localRotation = Quaternion.identity;
			});
		}

		public void SetupCardsInDiscard(IEnumerable<ICardView> newCardsInDiscardViews)
		{
			if (newCardsInDiscardViews == null)
				return;

			var rebuildSelf = false;
			var rebuildOpponent = false;
			foreach (var cardView in newCardsInDiscardViews)
			{
				rebuildSelf = rebuildSelf || cardView.IsSelf;
				rebuildOpponent = rebuildOpponent || !cardView.IsSelf;
				var container = cardView.IsSelf
					? gameContainers.GraveyardSelfContainer
					: gameContainers.GraveyardOpponentContainer;
				
				cardView.SelfContainer.SetAnchorsInCenter();
				cardView.SelfContainer.SetParent(container, false);
				cardView.SelfContainer.localRotation = Quaternion.identity;
				cardView.SelfContainer.DOKill(); // kill animations
			}

			if (rebuildSelf)
				LayoutRebuilder.ForceRebuildLayoutImmediate(gameContainers.GraveyardSelfContainer);
			
			if (rebuildOpponent)
				LayoutRebuilder.ForceRebuildLayoutImmediate(gameContainers.GraveyardOpponentContainer);
		}

		private async UniTask HandleManualEffectSelection(ICardView[] selfChangedCards)
		{
			foreach (var cardView in selfChangedCards)
			{
				var manualEffect = gameDatabase
					.GetEffects(cardView.RuntimeGameObject.Data.EffectsIds)
					.FirstOrDefault(e => e.TargetMod == EffectTargetMod.PlayerPicked
					                     && e.Phases.Contains(EffectPhase.AfterSpawn));

				if (manualEffect == default)
					continue;

				var allowedTargets = gameLogicContext.TargetConditionRepository
					.GetAllowedTargets(cardView.RuntimeGameObject, manualEffect);

				if (allowedTargets.Length == 0)
					continue;

				var param = new PerformEffectArgs {EffectDataId = manualEffect.Id};
				var model = new CmdParamsModel(gameContext.Timer.RuntimeData.TimeHash, param)
				{
					ExecutorObjectId = cardView.RuntimeData.Id
				};

				if (allowedTargets.Length == 1 && allowedTargets.Contains(cardView.RuntimeGameObject))
				{
					model.TargetObjectsIds.Add(cardView.RuntimeData.Id);
				}
				else
				{
					try
					{
						var pickInfo = new PickInfo(cardView, manualEffect.Id, manualEffect.MaxTargetCount);
						var targets = await manualArrowSystem.GetTargetsAsync(pickInfo);
						model.TargetObjectsIds.AddRange(targets.Select(x => x.RuntimeData.Id));
					}
					catch (OperationCanceledException)
					{
						var selfObject = cardView.RuntimeGameObject;
						if (gameLogicContext.TargetConditionRepository.IsAllowedTarget(selfObject, selfObject, manualEffect, selfObject))
							model.TargetObjectsIds.Add(selfObject.RuntimeData.Id);
					}
				}

				if (model.TargetObjectsIds.Count > 0)
					gameHub.PerformCommandAsync<PerformEffectCmd>(model).Forget();
			}
		}
		
		private UniTask RearrangeTableAsync(ICardView[] selfCards, ICardView[] opponentCards)
		{
			var tasks = new List<UniTask>();
			if (!selfCards.IsNullOrEmpty())
				tasks.Add(RearrangeTableAsync(false, Owner.Self));

			if (!opponentCards.IsNullOrEmpty())
				tasks.Add(RearrangeTableAsync(false, Owner.Opponent));

			return UniTask.WhenAll(tasks);
		}

		public UniTask RearrangeTableAsync(bool force, Owner owner, CancellationToken? token = null)
		{
			if (!token.HasValue)
			{
				switch (owner)
				{
					case Owner.Opponent:
						rearrangeTableOpponent?.Cancel();
						rearrangeTableOpponent?.Dispose();
						rearrangeTableOpponent = new CancellationTokenSource();
						token = rearrangeTableOpponent.Token;
						break;
					case Owner.Self:
						rearrangeTableSelf?.Cancel();
						rearrangeTableSelf?.Dispose();
						rearrangeTableSelf = new CancellationTokenSource();
						token = rearrangeTableSelf.Token;
						break;
					case Owner.None:
					default:
						token = CancellationToken.None;
						break;
				}
			}

			var cardsOnTableViews = GetCardsInTable(owner);
			if (cardsOnTableViews.IsNullOrEmpty())
				return UniTask.CompletedTask;

			var cardRect = cardsOnTableViews.First().Layout.SelfContainer.rect;
			var positions =
				tableCardsPositioning.CalculatePositions(cardsOnTableViews.Length, cardRect.width, cardRect.height,
					owner);
			var targets = cardsOnTableViews.ZipToDictionary(positions);
			var duration = force ? 0f : 0.5f;
			return animatorApplication.RestorePositionsAsync(targets, token.Value, duration:duration);
		}

		private UniTask RearrangeHandCardsAsync(ICardView[] selfCards)
		{
			var tasks = new List<UniTask>();
			if (selfCards.Length > 0)
				tasks.Add(RearrangeHandCardsAsync(Owner.Self));

			return UniTask.WhenAll(tasks);
		}

		private UniTask RearrangeHandCardsAsync(Owner owner)
		{
			return RearrangeHandCardsAsync(false, owner, GetCardsInHand(owner));
		}

		public UniTask RearrangeHandCardsAsync(bool force, Owner owner, params ICardView[] cardViews)
		{
			if (cardViews.IsNullOrEmpty())
				return UniTask.CompletedTask;
			cardViews = cardViews.OrderBy(x => x.RuntimeData.RelativePositionX).ToArray();
			var positions = owner == Owner.Self
				? selfHandCardsPositioning.CalculatePositions(cardViews.Length)
				: opponentHandCardsPositioning.CalculatePositions(cardViews.Length);
			var targets = cardViews.ZipToDictionary(positions);
			var duration = force ? 0f : 0.55f;
			return animatorApplication.RearrangeAsync(targets, Token, duration:duration);
		}

		public UniTask RearrangeInShowAllAsync(ICardView[] targets)
		{
			if (targets == null || targets.Length == 0)
				return UniTask.CompletedTask;

			var effectData = new EffectDataMock(EffectVisualKeyword.ArrangeInShowAll);
			var target = gameRepository.GetHeroByUserId(gameRepository.SelfId);
			var model = effectModelFatory.Create(effectData, target, null, targets.ToArray<IRuntimeObjectView>());
			return sequenceApplication.PlaySingleSequenceAsync(model);
		}

		private ICardView[] GetCardsInHand(Owner owner)
		{
			var userId = gameRepository.GetUserIdByOwner(owner);
			return gameContext.GameRuntimePool
				.GetCardsFilterBy(RuntimeState.InHand, userId)
				.Select(c => gameRepository.GetCardViewByRuntimeId(c.RuntimeData.Id))
				.OrderBy(c => c.RuntimeData.RelativePositionX)
				.ToArray();
		}

		private ICardView[] GetCardsInTable(Owner owner)
		{
			return gameContext.GameRuntimePool
				.GetCardsFilterBy(RuntimeState.InTable, gameRepository.GetUserIdByOwner(owner), ObjectType.TableCardsMask)
				.Select(c => gameRepository.GetCardViewByRuntimeId(c.RuntimeData.Id))
				.OrderBy(c => c.RuntimeData.RelativePositionX)
				.ToArray();
		}
	}

}