using System.Collections.Generic;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Game;
using BerserkV3.GameCore.UI;

namespace BerserkV3.GameCore.EffectsVisual.Abstractions
{
	public interface IRuntimeEffectModelFatory
	{
		IRuntimeEffectModel Create(
			EffectData data, 
			IRuntimeObjectView executor, 
			IEnumerable<IEffectRuntimeArg> args = null, 
			params IRuntimeObjectView[] targets);
		IRuntimeEffectModel Create(IRuntimeEffectData runtimeData, EffectData data);
		IRuntimeEffectModel Create(IRuntimeEffectData runtimeData);
	}
}