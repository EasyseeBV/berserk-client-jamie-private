using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.EffectSystem.Effects.RuntimeArgs;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.Stasis)]
	public class StasisEffect : KeywordEffect
	{
		public override void Create()
		{
			base.Create();
			foreach (var target in GetExecutionTargets())
			{
				foreach (var effect in target.AppliedEffects.Where(x => x.RuntimeData.Id != RuntimeData.Id))
				{
					RuntimeData.RuntimeArgs.Add(new StasisEffectArg
					{
						DisabledLength = effect.RuntimeData.DisabledLength,
						EffectId = effect.RuntimeData.Id,
						EffectOwnerId = target.RuntimeData.Id
					});
				}

				var stasisEffectArgs = RuntimeData.GetRuntimeArgs<StasisEffectArg>();
				if (!stasisEffectArgs.Any())
					RuntimeData.RuntimeArgs.Add(new StasisEffectArg
					{
						DisabledLength = 0,
						EffectId = -1,
						EffectOwnerId = target.RuntimeData.Id
					});
			}
		}

		protected override void OnExecute()
		{
			base.OnExecute();
			var args = RuntimeData.GetRuntimeArgs<StasisEffectArg>().ToArray();
			foreach (var argGroup in args.GroupBy(x => x.EffectOwnerId))
			{
				if (!GameContext.GameRuntimePool.TryGet(argGroup.Key, out var target))
					continue;

				foreach (var arg in argGroup)
				{
					if (!target.TryGetAppliedEffect(arg.EffectId, out var effect))
						continue;

					effect.Disable(RuntimeData.CurrentLength);
				}

				target.ChangeEffectPhase(EffectPhase.AfterDisable, damageType: EffectData.DamageType);
			}
		}

		protected override void OnExpire()
		{
			base.OnExpire();
			foreach (var group in RuntimeData.GetRuntimeArgs<StasisEffectArg>().GroupBy(x => x.EffectOwnerId))
			{
				if (!GameContext.GameRuntimePool.TryGet(group.Key, out var target))
					continue;

				foreach (var runtimeArg in group)
				{
					if (!target.TryGetAppliedEffect(runtimeArg.EffectId, out var effect))
						continue;

					effect.Disable(runtimeArg.DisabledLength);
					target.TryDie(Executor, EffectData.DamageType);
				}

				target.ChangeEffectPhase(EffectPhase.AfterDisable, damageType: EffectData.DamageType);
			}
		}
	}
}