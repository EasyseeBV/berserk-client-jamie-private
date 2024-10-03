using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.Stealthing)]
	public class StealthingEffect : KeywordEffect
	{
		protected override void OnExecute()
		{
			Executor.RemoveAppliedEffect(RuntimeData.Id);
		}
	}
}