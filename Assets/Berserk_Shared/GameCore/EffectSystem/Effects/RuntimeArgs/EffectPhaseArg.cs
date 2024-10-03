using System;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.RuntimeArgs
{
	[Serializable]
	public class EffectPhaseArg : IEffectRuntimeArg
	{
		public EffectPhase EffectPhase { get; }

		public EffectPhaseArg(EffectPhase effectPhase)
		{
			EffectPhase = effectPhase;
		}
	}
}