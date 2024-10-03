using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.RuntimeArgs
{
	public class TemporaryAppliedEffectArg : IEffectRuntimeArg
	{
		public int TargetId { get; }
		public int EffectId { get; }
		
		public TemporaryAppliedEffectArg(int targetId, int effectId)
		{
			TargetId = targetId;
			EffectId = effectId;
		}
	}
}