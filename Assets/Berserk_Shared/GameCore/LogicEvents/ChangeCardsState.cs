using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Newtonsoft.Json;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public class ChangeCardsState : LogicEvent
	{
		public RuntimeState OldState;
		public RuntimeState NewState;
		public List<int> CardIds;

		public ChangeCardsState()
		{
			CardIds = new List<int>();
		}
		
		[JsonConstructor]
		public ChangeCardsState(RuntimeState oldState, RuntimeState newState, params int[] cardIds)
		{
			OldState = oldState;
			NewState = newState;
			CardIds = cardIds.ToList();
		}
	}
}