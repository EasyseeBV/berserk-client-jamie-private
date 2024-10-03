using System;

namespace Berserk.Shared.GameCore.Exceptions
{
	public class CancelCommandException : Exception
	{
		public CancelCommandException(string msg) : base(msg){}
	}
}