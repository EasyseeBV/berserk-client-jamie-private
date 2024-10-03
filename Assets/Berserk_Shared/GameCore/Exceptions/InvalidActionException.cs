using System;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.GameCore.Exceptions
{
	public class InvalidActionException : Exception
	{
		public InvalidAction Value { get; } = InvalidAction.None;

		public InvalidActionException(InvalidAction value) : base(value.ToString())
		{
			Value = value;
		}

		public InvalidActionException(string message) : base(message){}

		public InvalidActionException(InvalidAction value, string message) : base(message)
		{
			Value = value;
		}
	}
}