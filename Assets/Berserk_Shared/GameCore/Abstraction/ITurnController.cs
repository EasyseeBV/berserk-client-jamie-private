using System;

namespace Berserk.Shared.GameCore.Abstraction
{
	public interface ITurnController : IDisposable
	{
		void Init(bool subscribe = true);
	}
}