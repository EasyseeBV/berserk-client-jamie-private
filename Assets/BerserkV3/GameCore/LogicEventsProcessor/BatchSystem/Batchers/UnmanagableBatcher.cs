using System.Collections.Generic;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicEvents;

namespace BerserkV3.GameCore.LogicEventsProcessor.BatchSystem
{
	internal class UnmanagableBatcher : BatcherBase
	{
		public UnmanagableBatcher(IBatchUnpacker batchUnpacker) : base(batchUnpacker)
		{
		}

		public override bool ShouldPerformBatching(IList<ILogicEvent> mainQueue)
		{
			return true;
		}

		public override void Process(IList<ILogicEvent> mainQueue)
		{
			BatchSimple<ChangedNextDeckCard>(mainQueue);
			Batch<ChangeDeckCount>(mainQueue, false);
		}
	}
}
