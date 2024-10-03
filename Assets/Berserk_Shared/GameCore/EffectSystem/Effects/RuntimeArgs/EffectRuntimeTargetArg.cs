using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.RuntimeArgs
{
	public class EffectRuntimeTargetArg : IEffectRuntimeArg
	{
		public int TargetId { get; }
		public EffectRuntimeTargetArg(int targetId)
		{
			TargetId = targetId;
		}
	}
}