using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public class ChangedTimer : LogicEvent
	{
		public IRuntimeTimerData RuntimeData { get; }
		
		public ChangedTimer(IRuntimeTimerData runtimeData)
		{
			RuntimeData = runtimeData.Clone(); // capture the state
		}
	}
}