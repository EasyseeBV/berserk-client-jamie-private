using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.Controllers
{
	public class TutorialGiveCardsService : DefaultGiveCardsService
	{
		public TutorialGiveCardsService(
			IGameContext gameContext,
			IGameLogicContext gameLogicContext)
			: base(gameContext, gameLogicContext) {}

		public override IEnumerable<IRuntimeGameObject> GiveStartingCardsToPlayer(string userId)
		{
			if (GameContext.RuntimeData.MatchMode != MatchMode.Tutorial)
				return base.GiveStartingCardsToPlayer(userId);
			
			var tutorialIds = GetTutorialIds(GameContext.SharedConfig.HandCardsStartCount, i => GetStartingCardId(userId, i));
			
			if (TryGetTutorialCardIds(userId, tutorialIds, RuntimeState.InDeck, out var newCards))
				return newCards.Select(card =>
				{
					card.RuntimeData.ResetRelativePositionX();
					card.ReturnToChoose();
					return card;
				}).ToArray();
			
			DefaultSharedLogger.Error($"[{GetType().Name}] Starting cards for user : {userId} wasn't setup!");
			return Array.Empty<IRuntimeGameObject>();
		}

		public override void GiveCards(string userId, int count)
		{
			if (GameContext.RuntimeData.MatchMode != MatchMode.Tutorial)
			{
				base.GiveCards(userId, count);
				return;
			}
			
			var tutorialIds = GetTutorialIds(count, i => GetTurnCardId(userId, i));
			if (!TryGetTutorialCardIds(userId, tutorialIds, RuntimeState.InDeck, out var newCards))
			{
				base.GiveCards(userId, count);
				return;
			}
			
			GiveCardsInternal(userId, newCards.Length, newCards.Select(x=> x.RuntimeData.Id).ToArray());
		}
		
		public override IEnumerable<IRuntimeGameCard> ReplaceMulliganCards(string userId, params int[] replaceIds)
		{
			if (GameContext.RuntimeData.MatchMode != MatchMode.Tutorial)
				return base.ReplaceMulliganCards(userId, replaceIds);
			
			var replaceCards = GameContext.GameRuntimePool
				.GetCardsFilterBy(RuntimeState.InChoose, userId, runtimeIds:replaceIds)
				.OrderBy(x=> x.RuntimeData.RelativePositionX)
				.ToArray();
			
			var tutorialIds = GetTutorialIds(replaceCards.Length, i => GetReplaceCardId(userId, i));
			if (!TryGetTutorialCardIds(userId, tutorialIds, RuntimeState.InDeck, out var newCards))
				return base.ReplaceMulliganCards(userId, replaceIds);
			
			return ReplaceMulliganCardsInternal(replaceCards, newCards);
		}

		private static IEnumerable<string> GetTutorialIds(int count, Func<int, string> idProvider)
		{
			if (count <= 0 || idProvider == null)
				return Array.Empty<string>();
			
			return Enumerable.Range(0, count).Select(idProvider);
		}

		private bool TryGetTutorialCardIds(
			string userId, 
			IEnumerable<string> tutorIds,
			RuntimeState? fromState,
			out IRuntimeGameCard[] tutorCards)
		{
			tutorCards = Array.Empty<IRuntimeGameCard>();
			var tutorCardDatas = GameContext.GameDatabase.GetTutorialCards(tutorIds).ToList();
			if (tutorCardDatas.Count == 0)
				return false;
			
			var possibleCards = new List<IRuntimeGameCard>();
			var existCards = GameContext.GameRuntimePool.GetCardsFilterBy(fromState, userId).ToList();
			foreach (var tutorCard in tutorCardDatas)
			{
				try
				{
					var runtimeCard = existCards.First(x => x.Data.Id == tutorCard.CardId);
					existCards.Remove(runtimeCard);
					possibleCards.Add(runtimeCard);
				}
				catch (Exception)
				{
					DefaultSharedLogger.Log($"[{GetType().Name}] {tutorCard} Runtime or Tutor card doesn't exist.");
				}
			}

			tutorCards = possibleCards.ToArray();
			return tutorCards.Length > 0;
		}

		private string GetStartingCardId(string userId, int cardIndex)
		{
			return $"{GetUserTag(userId)}_{nameof(TimerState.Mulligan)}_{cardIndex}";
		}

		private string GetReplaceCardId(string userId, int cardIndex)
		{
			return $"{GetUserTag(userId)}_{nameof(TimerState.Mulligan)}_Replace_{cardIndex}";
		}

		private string GetTurnCardId(string userId, int cardIndex)
		{
			return GameContext.Timer.RuntimeData != null
				? $"{GetUserTag(userId)}_{nameof(GameContext.Timer.RuntimeData.Turn)}_{GameContext.Timer.RuntimeData.Turn}_Index_{cardIndex}"
				: string.Empty;
		}

		private string GetUserTag(string userId)
		{
			if (!GameContext.PlayerRepository.TryGet(userId, out var player))
				return string.Empty;
			
			return player.RuntimeData.IsBot ? "Bot" : "User";
		}
	}
}