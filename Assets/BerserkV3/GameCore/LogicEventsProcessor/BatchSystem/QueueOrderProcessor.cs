using System.Collections.Generic;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.Utils;
using Sirenix.Utilities;

namespace BerserkV3.GameCore.LogicEventsProcessor.BatchSystem
{
	public interface IQueueOrderProcessor
	{
		void Process(List<ILogicEvent> mainQueue);
	}

	public class QueueOrderProcessor : DisposableWithCts, IQueueOrderProcessor
	{
		public void Process(List<ILogicEvent> mainQueue)
		{
			if (mainQueue.IsNullOrEmpty())
				return;
			
			SortByOrder(mainQueue);
		}
		
		private void SortByOrder<T>(IList<T> queue) where T : ILogicEvent
		{
			queue.Sort((x1, x2) => x1.Order.CompareTo(x2.Order));
		}
	}
}