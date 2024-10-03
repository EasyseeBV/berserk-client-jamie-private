using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public class AddObjectEffect : LogicEvent
	{
		public IRuntimeEffectData RuntimeData { get; }

		public AddObjectEffect(IRuntimeEffectData runtimeData)
		{
			RuntimeData = runtimeData.Clone(); // capture the state
		}
	}
}