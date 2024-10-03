using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public class DeleteObjectEffect : LogicEvent
	{
		public IRuntimeEffectData RuntimeData { get; }

		public DeleteObjectEffect(IRuntimeEffectData runtimeData)
		{
			RuntimeData = runtimeData.Clone(); // capture the state
		}
	}
}