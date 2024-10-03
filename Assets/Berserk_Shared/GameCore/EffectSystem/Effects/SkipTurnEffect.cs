using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.Stunning)]
	public class SkipTurnEffect : KeywordEffect
	{
		protected override void OnExecute()
		{
			foreach (var target in GetExecutionTargets())
			{
				target.RuntimeData.MoveCount.Set(0);
			}
		}
	}
}