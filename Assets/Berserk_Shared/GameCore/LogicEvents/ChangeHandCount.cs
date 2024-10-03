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
	public class ChangeHandCount : LogicEvent
	{
		public int HandCount;

		public ChangeHandCount()
		{
			
		}

		[JsonConstructor]
		public ChangeHandCount(int handCount)
		{
			HandCount = handCount;
		}
		
		public ChangeHandCount(string userId, IGameContext gameContext)
		{
			HandCount = 0;

			try
			{
				HandCount = gameContext.GameRuntimePool
					.GetCardsFilterBy(RuntimeState.InHand, userId, asQuery: true)
					.Count();
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
			}
		}
		
		public ChangeHandCount(string userId, IEnumerable<IRuntimeData> runtimePool)
		{
			try
			{
				HandCount = runtimePool
					.OfType<IRuntimeCardData>()
					.Count(x => x.State == RuntimeState.InHand && x.OwnerUserId == userId);
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
			}
		}
	}
}