using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.GameCore.Abstraction;

namespace BerserkV3.GameCore.LogicEventsProcessor.BatchSystem
{
	public class BatchersHolder
	{
		private readonly IBatcher[] batchers;

		public BatchersHolder(IBatchUnpacker batchUnpacker)
		{
			batchers = new IBatcher[]
			{
#if EVENT_BATCHING_ALL
				new AoeBatcher(batchUnpacker),
				new UnmanagableBatcher(batchUnpacker)
#endif
			};
		}

		public IEnumerable<IBatcher> GetBatchersReadyToExecute(IList<ILogicEvent> queue)
		{
			return batchers.Where(batcher => batcher.ShouldPerformBatching(queue));
		}
	}
}
