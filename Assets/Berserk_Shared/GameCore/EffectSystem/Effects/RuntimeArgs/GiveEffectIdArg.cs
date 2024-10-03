using System;
using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.RuntimeArgs
{
	[Serializable]
	public class GiveEffectIdArg : IEffectRuntimeArg
	{
		public string EffectConfigId { get; }
		public GiveEffectIdArg(string effectConfigId)
		{
			EffectConfigId = effectConfigId;
		}
	}
}