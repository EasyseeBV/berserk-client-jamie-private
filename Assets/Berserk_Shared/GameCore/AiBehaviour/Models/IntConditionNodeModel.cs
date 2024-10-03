using System;

namespace Berserk.Shared.GameCore.AiBehaviour.Models
{
	public enum ValueCondition
	{
		Equals,
		Less,
		More
	}

	[Serializable]
	public class IntConditionNodeModel
	{
		public int Value;
		public ValueCondition ValueCondition;

		public bool Check(int value)
		{
			switch (ValueCondition)
			{
				case ValueCondition.Equals:
					return Value == value;
				case ValueCondition.Less:
					return value < Value;
				case ValueCondition.More:
					return value > Value;
			}

			return false;
		}
	}
}