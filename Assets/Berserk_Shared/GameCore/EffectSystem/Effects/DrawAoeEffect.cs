using System;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.DrawAoe)]
	public class DrawAoeEffect : KeywordEffect
	{
		public override bool CanExecute()
		{
			if (!base.CanExecute() || RuntimeData.CurrentValue <= 0)
				return false;
			
			SetTargets(Targets.Shuffle().Take(RuntimeData.CurrentValue).ToArray());
			return base.CanExecute();
		}

		protected override void OnExecute()
		{
			if (!EffectData.TargetLimits.Contains(RuntimeState.InDeck))
				throw new InvalidOperationException("Can't draw non deck entities");
			
			var drawTargets = Targets.Select(x => x.RuntimeData.Id).ToArray();
			LogicContext.GiveCardsService.RequestGiveCards(Executor.RuntimeData.OwnerUserId, drawTargets.Length, drawTargets);
		}
	}
}