using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.GiveEffects
{
	[EffectKeyword(EffectKeyword.SleepGain)]
	[EffectKeyword(EffectKeyword.GiveTemporaryEffectsOnce)]
	public class GiveTemporaryEffectsOnceEffect : GiveTemporaryEffectsEffect
	{
		public override bool CanExecute()
		{
			return EffectData.MinTargetCount <= 0 || GetExecutionTargets().Length >= EffectData.MinTargetCount;
		}

		public override IRuntimeGameObject[] GetExecutionTargets()
		{
			var configIds = EffectData.GetConfigIdsFromMeta();
			return base.GetExecutionTargets().Where(target => FilterEffectIdsByTarget(target, configIds).Any()).ToArray();
		}

		protected override string[] FilterEffectIdsByTarget(IRuntimeGameObject target, params string[] configIds)
		{
			return configIds.Where(id => !target.HasAppliedEffect(id)).ToArray();
		}
	}
}