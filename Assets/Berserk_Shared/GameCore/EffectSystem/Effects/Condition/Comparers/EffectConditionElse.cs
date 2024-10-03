using Berserk.Shared.GameCore.EffectSystem.Effects.Condition.Data;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.Condition.Comparers
{
	[EffectCondition(EffectConditionType.Else)]
	public class EffectConditionElse : EffectConditionBase
	{
		public override bool Eval(params object[] args)
		{
			return true;
		}
	}
}