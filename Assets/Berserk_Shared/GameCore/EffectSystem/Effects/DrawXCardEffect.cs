using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.EffectSystem.Effects.RuntimeArgs;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.DrawXCard)]
	public class DrawXCardEffect : DrawEffect
	{
		public override void Create()
		{
			base.Create();
			RuntimeData.CurrentValue = RuntimeData.RuntimeArgs
				.OfType<EffectRuntimeTargetArg>()
				.Select(x => x.TargetId)
				.Distinct()
				.Count();
		}
		
		public override bool CanExecute()
		{
			return base.CanExecute() && RuntimeData.CurrentValue > 0;
		}
	}
}