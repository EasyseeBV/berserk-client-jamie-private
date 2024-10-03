using System.Collections.Generic;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicEvents;

namespace BerserkV3.GameCore.LogicEventsProcessor.BatchSystem
{
	public interface IBatchUnpacker
	{
		void RegisterBatchFlag<TBatch, TEvent>()
			where TBatch : BatchedLogicEvent<TEvent>
			where TEvent : ILogicEvent;

		IEnumerable<ILogicEvent> UnpackBatch(ILogicEvent batchedLogicEvent);
	}
}