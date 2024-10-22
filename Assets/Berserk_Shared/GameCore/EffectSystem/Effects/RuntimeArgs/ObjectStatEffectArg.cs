using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.RuntimeArgs
{
	public class ObjectStatEffectArg : IEffectRuntimeArg
	{
		public int RuntimeId { get; set; }
		public string StatName { get; set; }
		public int Value { get; set; }
	}
}