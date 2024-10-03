using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.GiveEffects
{
	[EffectKeyword(EffectKeyword.Pillager)]
	[EffectKeyword(EffectKeyword.Aspirant)]
	[EffectKeyword(EffectKeyword.GiveEffects)]
	public class GiveEffectsEffect : GiveEffectBase
	{
		protected override IRuntimeEffect GiveEffect(string effectConfigId, IRuntimeGameObject target)
		{
			var effectData = GameContext.GameDatabase.GetEffectConfig(effectConfigId);
			return effectData.Applied
				? LogicContext.EffectExecutor.CreateAppliedEffectAuto(effectConfigId, target, RuntimeData.RuntimeArgs)
				: LogicContext.EffectExecutor.CreateAndExecuteEffectAuto(effectConfigId, target, RuntimeData.RuntimeArgs, target);
		}
	}
}