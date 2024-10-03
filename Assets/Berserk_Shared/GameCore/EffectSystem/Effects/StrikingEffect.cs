using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.EffectSystem.Effects.RuntimeArgs;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.Striking)]
	public class StrikingEffect : MultipleAttackEffect
	{
		protected override void StartAttacks()
		{
			base.StartAttacks();
			if (RuntimeData.RuntimeArgs.Any())
				return;

			var exceptConfigId = GetEffectAttackId();
			RuntimeData.RuntimeArgs.AddRange(EffectData.GetConfigIdsFromMeta()
				.Where(id => id != exceptConfigId)
				.Select(id => LogicContext.EffectExecutor.CreateAppliedEffectAuto(id, Executor))
				.Select(e => new TemporaryAppliedEffectArg(e.Executor.RuntimeData.Id, e.RuntimeData.Id)));
		}

		protected override void EndAttacks()
		{
			base.EndAttacks();
			
			foreach (var arg in RuntimeData.GetRuntimeArgs<TemporaryAppliedEffectArg>())
				GameContext.GameRuntimePool.Get(arg.TargetId)?.RemoveAppliedEffect(arg.EffectId);
			
			RuntimeData.RuntimeArgs.Clear();
		}
	}
}