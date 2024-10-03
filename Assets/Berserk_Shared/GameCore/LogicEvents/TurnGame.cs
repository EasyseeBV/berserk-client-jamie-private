using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public class TurnGame : LogicEvent
	{
		public IRuntimeTimerData RuntimeData { get; }
		
		public TurnGame(IRuntimeTimerData runtimeData)
		{
			RuntimeData = runtimeData.Clone();  // capture the state
		}
	}
}