using System;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{

	[EffectKeyword(EffectKeyword.DiscardDeck)]
	public class DiscardDeckEffect : DiscardEffect
	{
		protected override void OnExecute()
		{
			var discardUserIds = GetExecutionTargets().Select(x => x.RuntimeData.OwnerUserId).Distinct().ToArray();
			foreach (var target in GetUsersDeckCards(ValueModRounded(), discardUserIds))
				Discard(target);
		}

		protected virtual IRuntimeGameCard[] GetUsersDeckCards(int count, params string[] userIds)
		{
			if (count <= 0)
				return Array.Empty<IRuntimeGameCard>();

			return userIds.Length == 0 
				? GetUserDeckCards(null, count) 
				: userIds.SelectMany(id => GetUserDeckCards(id, count)).ToArray();
		}

		protected virtual IRuntimeGameCard[] GetUserDeckCards(string userId, int count)
		{
			return GameContext.GameRuntimePool.GetCardsFilterBy(RuntimeState.InDeck, userId, asQuery: true)
				.Take(count)
				.ToArray();
		}
	}

}