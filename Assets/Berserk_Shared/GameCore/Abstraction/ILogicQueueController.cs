using System;

namespace Berserk.Shared.GameCore.Abstraction
{
	public interface ILogicQueueController : IDisposable
	{
		void Initialize();
		void Add(ILogicEvent logicEvent, string userId = null);
		void SendAndClearLogicQueue();
	}
}