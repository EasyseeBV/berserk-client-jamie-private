using System;

namespace Berserk.Shared.GameCore.Abstraction
{
	public interface ISessionProcessor : IDisposable
	{
		IGameContext Context { get; }
		IGameLogicContext LogicContext { get; }
		ISessionProcessor Build(params object[] args);
	}
}