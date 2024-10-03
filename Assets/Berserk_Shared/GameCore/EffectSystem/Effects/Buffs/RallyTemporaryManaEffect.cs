using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.Buffs
{
	[EffectKeyword(EffectKeyword.RallyTemporaryMana)]
	public class RallyTemporaryManaEffect : BuffStatReplaceManaEffect
	{
		protected override void OnExecute()
		{
			foreach (var target in GetExecutionTargets())
			{
				if (RuntimeData.AppliedIds.Contains(target.RuntimeData.Id)) 
					continue;
				
				target.OnPhaseChanged += OnTargetPhaseChanged;
			}
			base.OnExecute();
		}

		private void OnTargetPhaseChanged(EffectPhase phase, int runtimeId, DamageType damageType)
		{
			if (phase != EffectPhase.AfterSpawn)
				return;
			
			Executor.RemoveAppliedEffect(this);
		}

		protected override void OnExpire()
		{
			foreach (var target in GameContext.GameRuntimePool.GetMany(RuntimeData.AppliedIds))
				target.OnPhaseChanged -= OnTargetPhaseChanged;
			
			base.OnExpire();
		}
	}
}