using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.EffectSystem;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.Controllers
{
	public class DefaultGiveCardsService : IGiveCardsService
	{
		protected readonly IGameContext GameContext;
		protected readonly IGameLogicContext GameLogicContext;

		public DefaultGiveCardsService(IGameContext gameContext, IGameLogicContext gameLogicContext)
		{
			GameContext = gameContext;
			GameLogicContext = gameLogicContext;
		}

		public virtual IEnumerable<IRuntimeGameObject> GiveStartingCardsToPlayer(string userId)
		{
			return GameContext.GameRuntimePool
				.GetCardsFilterBy(RuntimeState.InDeck, userId)
				.Take(GameContext.SharedConfig.HandCardsStartCount)
				.Select(card =>
				{
					card.RuntimeData.ResetRelativePositionX();
					card.ReturnToChoose();
					return card;
				}).ToArray();
		}

		public virtual void GiveCards(string userId, int count)
		{
			GiveCardsInternal(userId, count);
		}

		public virtual void RequestGiveCards(string userId, int count, params int[] runtimeIds)
		{
			GiveCardsInternal(userId, count, runtimeIds);
		}

		public virtual IEnumerable<IRuntimeGameCard> ReplaceMulliganCards(string userId, params int[] replaceIds)
		{
			if (string.IsNullOrEmpty(userId))
			{
				DefaultSharedLogger.Error($"[{GetType().Name}] {nameof(ReplaceMulliganCards)} : User id is missing!");
				return Array.Empty<IRuntimeGameCard>();
			}
			
			if (replaceIds.Length == 0)
				return Array.Empty<IRuntimeGameCard>();

			var markedToReplaceCards = GameContext.GameRuntimePool
				.GetCardsFilterBy(RuntimeState.InChoose, userId, runtimeIds:replaceIds)
				.ToArray();

			if (markedToReplaceCards.Length == 0)
				return markedToReplaceCards;
			
			var mulliganCards = GameContext.GameRuntimePool
				.GetCardsFilterBy(RuntimeState.InChoose, userId)
				.ToArray();

			var newCardsFromDeck = GameContext.GameRuntimePool
				.GetCardsFilterBy(RuntimeState.InDeck, userId)
				.Except(mulliganCards)
				.Shuffle()
				.Take(markedToReplaceCards.Length)
				.ToArray();

			return ReplaceMulliganCardsInternal(markedToReplaceCards, newCardsFromDeck);
		}

		public void Shuffle(string userId)
		{
			if (string.IsNullOrEmpty(userId))
			{
				DefaultSharedLogger.Error("Can't shuffle deck, userId IsNullOrEmpty");
				return;
			}
			
			var deckCards = GameContext.GameRuntimePool
				.GetCardsFilterBy(RuntimeState.InDeck, userId)
				.ToArray();

			if (deckCards.Length == 0)
				return;
			
			var shuffled = deckCards.Shuffle().ToArray();

			GameContext.GameRuntimePool.RemoveRage(deckCards);
			GameContext.GameRuntimePool.AddRange(shuffled);
			
			UpdateNextDeckCard(userId);
		}

		public bool IsDeckEmpty(string userId)
		{
			return !GameContext.GameRuntimePool.GetCardsFilterBy(RuntimeState.InDeck, userId).Any();
		}

		protected IEnumerable<IRuntimeGameCard> ReplaceMulliganCardsInternal(IRuntimeGameCard[] replaceCards, IRuntimeGameCard[] deckCards)
		{
			if (deckCards.Length != replaceCards.Length)
			{
				DefaultSharedLogger.Error($"[{GetType().Name}] replace & deck cards must be equal count.");
				return Array.Empty<IRuntimeGameCard>();
			}
			
			for (var i = 0; i < replaceCards.Length; i++)
			{
				var fromReplace = replaceCards[i];
				var toReplace = deckCards[i];
				
				GameContext.GameRuntimePool.SetLastIndex(fromReplace);
				toReplace.RuntimeData.SetRelativePositionX(fromReplace.RuntimeData.RelativePositionX);
				
				fromReplace.ReturnToDeck();
				toReplace.ReturnToChoose();
			}

			return deckCards;
		}
		
		protected void GiveCardsInternal(string userId, int count, params int[] runtimeIds)
		{
			NotifyIfDeckEmpty(userId);
			GiveCards(GetPossibleToDraw(userId, count, runtimeIds));
			UpdateNextDeckCard(userId);
		}

		private void NotifyIfDeckEmpty(string userId)
		{
			if (!IsDeckEmpty(userId))
				return;
			
			GameLogicContext.LogicQueueController.Add(new InvalidActionEvent(InvalidAction.DeckIsEmpty), userId);
			ExecuteEffectIfDeckEmpty(userId);
		}
		
		private void ExecuteEffectIfDeckEmpty(string userId)
		{
			var effectId = EffectKeyword.DeckDepleted.AsSystemEffectId();
			var target = GameContext.GameRuntimePool.GetHeroByUserId(userId);
			GameLogicContext.EffectExecutor.CreateAndExecuteEffect(effectId, target, targets: target);
		}
		
		public void UpdateNextDeckCard(string userId)
		{
			GameLogicContext.LogicQueueController.Add(new ChangedNextDeckCard(userId, GameContext.GameRuntimePool), userId);
		}

		private void GiveCards(params IRuntimeGameCard[] drawCards)
		{
			if (drawCards is not {Length: > 0})
				return;
			
			foreach (var gameCard in drawCards)
			{			
				gameCard.ReturnToHand();
				gameCard.ChangeEffectPhase(EffectPhase.AfterDraw);
			}
		}

		private IRuntimeGameCard[] GetPossibleToDraw(string userId, int count, params int[] runtimeIds)
		{
			var player = GameContext.PlayerRepository.Get(userId);
			var handCount = (int)player.RuntimeData.HandCount;
			var handCards = GameContext.GameRuntimePool
				.GetCardsFilterBy(RuntimeState.InHand, userId)
				.ToArray();
			
			if (handCards.Length >= handCount
			    || !RestrictCount(handCards.Length, count, handCount, out var possibleToDraw))
				return Array.Empty<IRuntimeGameCard>();
			
			return GameContext.GameRuntimePool
				.GetCardsFilterBy(RuntimeState.InDeck, userId, runtimeIds:runtimeIds)
				.Take(possibleToDraw)
				.ToArray();
		}

		private bool RestrictCount(int handCount, int addCount, int limit, out int result)
		{
			var total = handCount + addCount;
			if (addCount <= 0 || total <= limit)
			{
				result = addCount;
				return result > 0;
			}
			
			var except = total - limit;
			result = Math.Clamp(addCount - except, 0, addCount);
			return result > 0;
		}
	}
}