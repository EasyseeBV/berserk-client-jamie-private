using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public class ChangeObjectEffect : LogicEvent
	{
		public IRuntimeEffectData RuntimeData { get; }
		
		public ChangeObjectEffect(IRuntimeEffectData runtimeData)
		{
			RuntimeData = runtimeData.Clone();  // capture the state
		}
	}
}