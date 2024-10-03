using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public class StartEffect : LogicEvent
	{
		public IRuntimeEffectData RuntimeData { get; }

		public StartEffect(IRuntimeEffectData runtimeData)
		{
			RuntimeData = runtimeData.Clone(); // capture the state
		}
	}
}