using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.RuntimeArgs
{
	public class ObjectStatEffectArg : IEffectRuntimeArg
	{
		public int RuntimeId { get; set; }
		public string StatName { get; set; }
		public int Default { get; set; }
		public int From { get; set; }
		public int To { get; set; }
	}
}