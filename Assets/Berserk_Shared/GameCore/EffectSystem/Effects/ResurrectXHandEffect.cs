using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.EffectSystem.Effects.RuntimeArgs;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.ResurrectXHand)]
	public class ResurrectXHandEffect : ResurrectEffect
	{
		
		protected override bool RemoveIresurrectables => false;

		public override void Create()
		{
			base.Create();
			RuntimeData.AccessLevel |= AccessLevel.Self;
			
			if (RuntimeData.CurrentValue <= 0)
				RuntimeData.CurrentValue = RuntimeData
					.GetRuntimeArgs<EffectRuntimeTargetArg>()
					.Select(x => x.TargetId)
					.Distinct()
					.Count();
		}

		public override bool CanExecute()
		{
			return base.CanExecute() && RuntimeData.CurrentValue > 0;
		}
		
		protected override void OnExecute()
		{
			foreach (var target in Targets.OfType<IRuntimeGameCard>())
				Resurrect(target, true);
		}
		
		protected override void OnResurrect(IRuntimeGameCard target)
		{
			target.ReturnToHand();
		}
	}
}