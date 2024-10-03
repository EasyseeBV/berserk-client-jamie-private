using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.SuffleDeck)]
	public class SuffleDeckEffect : KeywordEffect
	{
		protected override void OnExecute()
		{
			LogicContext.GiveCardsService.Shuffle(Executor.RuntimeData.OwnerUserId);
		}
	}
}