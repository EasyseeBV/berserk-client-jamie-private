using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.RuntimeArgs
{
	public class OverchargeRuntimeArg : IEffectRuntimeArg
	{
		public int RuntimeEffectId { get; set; }
		public int Count { get; set; }
	}
}