using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.RuntimeArgs
{
	public class StasisEffectArg : IEffectRuntimeArg
	{
		public int EffectOwnerId { get; set; }
		public int EffectId { get; set; }
		public int DisabledLength { get; set; }
	}
}