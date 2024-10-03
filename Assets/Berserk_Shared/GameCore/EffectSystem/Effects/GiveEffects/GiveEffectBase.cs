using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.EffectSystem.Effects.RuntimeArgs;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.GiveEffects
{
	public abstract class GiveEffectBase : KeywordEffect
	{
		protected override void OnExecute()
		{
			DealEffects(GetExecutionTargets());
		}
		
		protected abstract IRuntimeEffect GiveEffect(string effectConfigId, IRuntimeGameObject target);
		
		protected virtual IEnumerable<IRuntimeEffect> DealEffects(params IRuntimeGameObject[] targets)
		{
			var effectIds = EffectData.Meta;
			if (string.IsNullOrWhiteSpace(effectIds))
				effectIds = string.Join(",",RuntimeData.GetRuntimeArgs<GiveEffectIdArg>().Select(x=> x.EffectConfigId));
			
			return DealEffects(effectIds, targets);
		}
		
		protected virtual IEnumerable<IRuntimeEffect> DealEffects(string effectIds, params IRuntimeGameObject[] targets)
		{
			if (targets == null || !targets.Any())
				return Array.Empty<IRuntimeEffect>();
			
			if (string.IsNullOrEmpty(effectIds))
				throw new NullReferenceException($"[{GetType().Name}] {nameof(effectIds)} {nameof(string.IsNullOrEmpty)} by {EffectData.Id}");

			var configIds = effectIds.GetConfigIds();
			
			return targets
				.SelectMany(target => FilterEffectIdsByTarget(target, configIds).Select(id => DealEffect(id, target)))
				.ToArray();
		}

		protected virtual IRuntimeEffect DealEffect(string effectConfigId, IRuntimeGameObject target)
		{
			if (target == null)
				throw new NullReferenceException($"[{GetType().Name}] Target is missing for effect config id: {effectConfigId}");
			
			if (string.IsNullOrEmpty(effectConfigId))
				throw new NullReferenceException($"[{GetType().Name}] {nameof(effectConfigId)} {nameof(string.IsNullOrEmpty)} by {EffectData.Id}");
			
			return GiveEffect(effectConfigId, target);
		}

		protected virtual string[] FilterEffectIdsByTarget(IRuntimeGameObject target, params string[] configIds)
		{
			return configIds;
		}
	}
}