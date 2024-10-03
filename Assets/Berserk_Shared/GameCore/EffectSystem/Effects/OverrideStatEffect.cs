using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.OverrideStat)]
	public class OverrideStatEffect : KeywordEffect
	{
		protected override void OnExecute()
		{
			foreach (var target in GetExecutionTargets())
			{
				foreach (var stat in EffectData.GetStatsFromMeta(target))
				{
					var value = ValueModRounded(stat);
					stat.SetMax(value).Set(value);
				}
				
				target.TryDie(Executor, EffectData.DamageType);
			}
		}
	}
}