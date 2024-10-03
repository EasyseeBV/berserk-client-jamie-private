using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public class EndGame : LogicEvent, ISharedEvent
	{
		public string WinnerId;
		public string LooserId;
		public GameEndReason Reason;

		public EndGame(string winnerId, string looserId, GameEndReason reason)
		{
			WinnerId = winnerId;
			LooserId = looserId;
			Reason = reason;
		}
	}
}