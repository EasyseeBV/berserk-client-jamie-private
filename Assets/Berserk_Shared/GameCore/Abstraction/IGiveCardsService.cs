using System.Collections.Generic;

namespace Berserk.Shared.GameCore.Abstraction
{
	public interface IGiveCardsService
	{
		IEnumerable<IRuntimeGameObject> GiveStartingCardsToPlayer(string userId);
		/// <summary>
		/// Use for game actions like Turn changed
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="count"></param>
		/// <param name="runtimeIds"></param>
		void GiveCards(string userId, int count);
		/// <summary>
		/// Use this instead GiveCards to Draw by effects or for custom request
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="count"></param>
		/// <param name="runtimeIds"></param>
		void RequestGiveCards(string userId, int count, params int[] runtimeIds);
		IEnumerable<IRuntimeGameCard> ReplaceMulliganCards(string userId, params int[] replaceIds);
		void UpdateNextDeckCard(string userId);
		void Shuffle(string userId);
		bool IsDeckEmpty(string userId);
	}
}