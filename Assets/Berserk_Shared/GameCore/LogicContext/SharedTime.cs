using System;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.LogicContext
{

	public class SharedTime : ISharedTime
	{
		public virtual DateTime Current => DateTime.UtcNow;
	}

}