using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.Utils;
using Newtonsoft.Json;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public class ChangeDeckCount : LogicEvent
	{
		public int DeckCount;
		public string UserId;
		
		[JsonConstructor]
		public ChangeDeckCount(int deckCount, string userId)
		{
			DeckCount = deckCount;
			UserId = userId;
		}

		public ChangeDeckCount(string userId, IGameContext gameContext)
		{
			try
			{
				UserId = userId;
				DeckCount = gameContext.GameRuntimePool
					.GetCardsFilterBy(RuntimeState.InDeck, userId, asQuery: true)
					.Count();
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
			}
		}

		public ChangeDeckCount(string userId, IEnumerable<IRuntimeData> runtimePool)
		{
			try
			{
				UserId = userId;
				DeckCount = runtimePool
					.OfType<IRuntimeCardData>()
					.Count(x => x.State == RuntimeState.InDeck && x.OwnerUserId == userId);
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
			}
		}
	}
}