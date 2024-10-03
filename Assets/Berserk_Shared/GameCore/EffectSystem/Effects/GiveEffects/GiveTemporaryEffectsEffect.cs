using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.EffectSystem.Effects.RuntimeArgs;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.GiveEffects
{
	[EffectKeyword(EffectKeyword.GiveTemporaryEffects)]
	public class GiveTemporaryEffectsEffect : GiveEffectsEffect
	{
		protected override void OnExecute()
		{
			RuntimeData.RuntimeArgs.AddRange(GetExecutionTargets()
				.SelectMany(t => DealEffects(t).Select(e => new TemporaryAppliedEffectArg(t.RuntimeData.Id, e.RuntimeData.Id))));
		}

		protected override void OnExpire()
		{
			var query = RuntimeData.GetRuntimeArgs<TemporaryAppliedEffectArg>()
				.Select(x => (x, GameContext.GameRuntimePool.Get(x.TargetId)));
			
			foreach (var (arg, target) in query)
				target.RemoveAppliedEffect(arg.EffectId);
			
			RuntimeData.RuntimeArgs.Clear();
			base.OnExpire();
		}
	}
}