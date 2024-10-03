using System;
using Berserk.Shared.Data.Enums;

namespace Vulcan.VFX
{
	[Serializable]
	public sealed class VFXKey
	{
		public static readonly string None = "None";
		public static readonly string Any = "Any";

		public string EntityId = Any;
		public EffectVisualKeyword Effect = EffectVisualKeyword.None;

		public override string ToString() => $"[{EntityId}:{Effect}]";
	}
}