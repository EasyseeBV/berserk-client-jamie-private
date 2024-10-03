using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public class CreateObject : LogicEvent
	{
		public IRuntimeData RuntimeData;

		public CreateObject(IRuntimeData runtimeData)
		{
			RuntimeData = runtimeData.Clone(); // capture the state
		}
	}
}