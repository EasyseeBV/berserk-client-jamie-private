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
	public class ChangedNextDeckCard : LogicEvent
	{
		public string CardId;

		[JsonConstructor]
		public ChangedNextDeckCard(string cardId)
		{
			CardId = cardId;
		}

		public ChangedNextDeckCard(string userId, IGameRuntimePool gameRuntimePool)
		{
			try
			{
				CardId = gameRuntimePool
					.GetCardsFilterBy(RuntimeState.InDeck, userId)
					.FirstOrDefault()?.Data?.Id;
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
			}
		}

		public ChangedNextDeckCard(string userId, IEnumerable<IRuntimeData> runtimePool)
		{
			try
			{
				CardId = runtimePool
					.OfType<IRuntimeCardData>()
					.FirstOrDefault(x => x.State == RuntimeState.InDeck && x.OwnerUserId == userId)?.DataId;
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
			}
		}
	}
}