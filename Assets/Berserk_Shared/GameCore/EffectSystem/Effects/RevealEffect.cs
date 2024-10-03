using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{

	[EffectKeyword(EffectKeyword.Reveal)]
	public class RevealEffect : KeywordEffect
	{
		public override bool CanExecute()
		{
			if (!base.CanExecute())
				return false;

			var targetEffects = EffectData.GetConfigIdsFromMeta();
			if (targetEffects == null || targetEffects.Length == 0)
				return false;

			SetTargets(Targets.Where(x => targetEffects.Any(x.HasAppliedEffect)).ToArray());
			return base.CanExecute();
		}

		protected override void OnExecute()
		{
			var targetEffects = EffectData.GetConfigIdsFromMeta();
			if (targetEffects == null || targetEffects.Length == 0)
				return;

			foreach (var target in GetExecutionTargets())
			{
				foreach (var configEffectId in targetEffects)
					target.RemoveAppliedEffect(configEffectId);
			}
		}
	}
}