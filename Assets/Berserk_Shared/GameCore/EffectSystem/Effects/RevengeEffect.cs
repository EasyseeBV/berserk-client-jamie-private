using System;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.Revenge)]
	public class RevengeEffect : KeywordEffect
	{
		public override bool CanExecute()
		{
			return EffectData.MinTargetCount <= 0 || GetExecutionTargets().Length >= EffectData.MinTargetCount;
		}

		public override IRuntimeGameObject[] GetExecutionTargets()
		{
			var configIds = EffectData.GetConfigIdsFromMeta();
			return base.GetExecutionTargets()
				.Where(t => t.RuntimeData.AppliedEffects.Any(a => configIds.Contains(a.ConfigId))
				            || t.RuntimeData.ImposingEffects.Any(configIds.Contains))
				.ToArray();
		}

		protected override void OnExecute()
		{
			foreach (var target in GetExecutionTargets())
			{
				ExecuteTargetEffects(target);
			}
		}

		private void ExecuteTargetEffects(IRuntimeGameObject target)
		{
			if (target.HasEffectsDisable())
				return;
			
			var configIds = EffectData.GetConfigIdsFromMeta();
			foreach (var appliedEffect in target.AppliedEffects.ToArray().Where(a => configIds.Contains(a.RuntimeData.ConfigId) && !a.RuntimeData.Disabled))
			{
				LogicContext.EffectExecutor.ExecuteAppliedEffect(appliedEffect);
			}

			var impossingEffectIds = target.RuntimeData.ImposingEffects.Where(configIds.Contains);
			foreach (var effectData in GameContext.GameDatabase.GetEffects(impossingEffectIds))
			{
				GiveEffect(effectData, target);
			}
		}

		protected virtual IRuntimeEffect GiveEffect(EffectData effectData, IRuntimeGameObject target)
		{
			if (target == null)
				throw new NullReferenceException($"[{GetType().Name}] Target is missing for effect config id: {effectData.Id}");

			if (string.IsNullOrEmpty(effectData.Id))
				throw new NullReferenceException($"[{GetType().Name}] {nameof(effectData.Id)} {nameof(string.IsNullOrEmpty)} by {EffectData.Id}");

			if (!effectData.Applied)
				return LogicContext.EffectExecutor.CreateAndExecuteEffectAuto(effectData.Id, target, null, target);

			target.RemoveImpossingEffects(effectData.Id);
			return LogicContext.EffectExecutor.CreateAndExecuteEffectAuto(effectData.Id, target, null, target);
		}
	}
}