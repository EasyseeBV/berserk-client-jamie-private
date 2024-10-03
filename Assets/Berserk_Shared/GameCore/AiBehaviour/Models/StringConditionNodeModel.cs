using System;

namespace Berserk.Shared.GameCore.AiBehaviour.Models
{
	[Serializable]
	public class StringConditionNodeModel
	{
		public string Value = string.Empty;
		public string Condition = string.Empty;

		public bool Check(string value)
		{
			return Condition.ToLower() switch
			{
				"equals" => Value == value,
				"less" => value.Length < Value.Length,
				"more" => value.Length > Value.Length,
				_ => false
			};
		}
	}
}