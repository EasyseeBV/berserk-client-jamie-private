using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public class EndEffect : LogicEvent
	{
		public IRuntimeEffectData RuntimeData { get; }

		public EndEffect(IRuntimeEffectData runtimeData)
		{
			RuntimeData = runtimeData.Clone(); // capture the state
		}
	}
}