using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.Discard)]
	public class DiscardEffect : KeywordEffect
	{
		protected override void OnExecute()
		{
			foreach (var target in GetExecutionTargets().OfType<IRuntimeGameCard>())
				Discard(target);
		}

		protected void Discard(IRuntimeGameCard target)
		{
			target.ChangeEffectPhase(EffectPhase.BeforeDiscard);
			OnDiscard(target);
			target.ChangeEffectPhase(EffectPhase.AfterDiscard);
		}

		protected virtual void OnDiscard(IRuntimeGameCard target)
		{
			target.ReturnToShowAll();
			target.ReturnToDiscard();
		}
	}
}