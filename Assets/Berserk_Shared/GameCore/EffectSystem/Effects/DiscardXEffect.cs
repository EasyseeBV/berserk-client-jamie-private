using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.EffectSystem.Effects.RuntimeArgs;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.DiscardX)]
	public class DiscardXEffect : DiscardEffect
	{
		public override void Create()
		{
			base.Create();
			RuntimeData.AccessLevel |= AccessLevel.Self;
		}

		protected override void OnDiscard(IRuntimeGameCard target)
		{
			target.ReturnToDiscard();
		}
		
		public override void OnDeleted() // when effect is applied and has targets to post execute
		{
			base.OnDeleted();
			if (GetExecutionTargets().Length == 0)
				return;
			
			GiveEffects();
		}

		protected virtual void GiveEffects()
		{
			var effectIds = EffectData.GetConfigIdsFromMeta();
			if (effectIds.Length == 0)
				return;

			var effExecutor = LogicContext.EffectExecutor;
			var effectId = EffectKeyword.GiveEffects.AsRuntimeEffectId();
			effectIds.Select(GetEffectArgs).ToList()
				.ForEach(args => effExecutor.CreateAndExecuteEffectAuto(effectId, Executor, args, Executor));
		}

		protected virtual IEnumerable<IEffectRuntimeArg> GetEffectArgs(string effectId)
		{
			return GetExecutionTargets()
				.Select(x => new EffectRuntimeTargetArg(x.RuntimeData.Id))
				.Append<IEffectRuntimeArg>(new GiveEffectIdArg(effectId));
		}
	}
}