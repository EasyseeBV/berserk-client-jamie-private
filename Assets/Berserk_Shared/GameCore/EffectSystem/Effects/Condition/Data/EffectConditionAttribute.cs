using System;

namespace Berserk.Shared.GameCore.EffectSystem.Effects.Condition.Data
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class EffectConditionAttribute : Attribute
	{
		public readonly EffectConditionType ConditionType;

		public EffectConditionAttribute(EffectConditionType conditionType)
		{
			ConditionType = conditionType;
		}
	}
}