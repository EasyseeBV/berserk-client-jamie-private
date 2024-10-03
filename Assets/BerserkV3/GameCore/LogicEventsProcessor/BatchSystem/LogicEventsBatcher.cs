using System.Collections.Generic;
using Berserk.Shared.GameCore.Abstraction;
using Sirenix.Utilities;

namespace BerserkV3.GameCore.LogicEventsProcessor.BatchSystem
{
	public class LogicEventsBatcher : ILogicEventsBatcher
	{
		private readonly IQueueOrderProcessor orderProcessor;
		private readonly IBatchUnpacker batchUnpacker;
		private readonly BatchersHolder batchersHolder;
		public LogicEventsBatcher(IQueueOrderProcessor orderProcessor)
		{
			this.orderProcessor = orderProcessor;
			batchUnpacker = new BatchUnpacker();
			batchersHolder = new BatchersHolder(batchUnpacker);
		}

		public void Process(List<ILogicEvent> mainQueue)
		{
			if (mainQueue.IsNullOrEmpty())
				return;

			batchersHolder
				.GetBatchersReadyToExecute(mainQueue)
				.ForEach(x => x.Process(mainQueue));

			orderProcessor.Process(mainQueue);
		}

		public IEnumerable<ILogicEvent> UnpackBatch(ILogicEvent logicEvent)
		{
			return batchUnpacker.UnpackBatch(logicEvent);
		}
	}
}