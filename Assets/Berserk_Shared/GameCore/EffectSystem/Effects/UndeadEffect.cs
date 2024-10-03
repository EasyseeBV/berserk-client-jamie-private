using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{

	[EffectKeyword(EffectKeyword.Undead)]
	public class UndeadEffect : RevealEffect
	{
		public override bool CanExecute()
		{
			return GameContext.GameRuntimePool.GetCardsFilterBy(RuntimeState.InTable, null, ObjectType.TableCardsMask, true)
				.All(o => !o.HasAppliedEffect(EffectKeyword.Sanctification)) && base.CanExecute();
		}
	}

}