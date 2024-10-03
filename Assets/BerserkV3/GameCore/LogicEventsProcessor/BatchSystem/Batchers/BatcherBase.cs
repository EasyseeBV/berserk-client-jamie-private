using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicEvents;
using Sirenix.Utilities;

namespace BerserkV3.GameCore.LogicEventsProcessor.BatchSystem
{
	internal abstract class BatcherBase : IBatcher
	{
		protected readonly IBatchUnpacker BatchUnpacker;

		public BatcherBase(IBatchUnpacker batchUnpacker)
		{
			BatchUnpacker = batchUnpacker;
		}

		public abstract bool ShouldPerformBatching(IList<ILogicEvent> mainQueue);
		public abstract void Process(IList<ILogicEvent> mainQueue);

		protected void BatchSimple<T>(IList<ILogicEvent> mainQueue, bool newerOrder = true) where T : ILogicEvent
		{
			var isolated = IsolateFrom<T>(mainQueue);
			var batch = newerOrder ? isolated.LastOrDefault() : isolated.FirstOrDefault();
			QueueInsert(mainQueue, batch);
		}

		protected void Batch<T>(IList<ILogicEvent> mainQueue, bool firstOrder = true) where T : ILogicEvent
		{
			var isolated = IsolateFrom<T>(mainQueue).ToList();
			if (isolated.Count == 0)
				return;
			
			var batch = new BatchedLogicEvent<T>
			{
				Order = firstOrder ? isolated.Min(x=> x.Order) : isolated.Max(x=> x.Order), 
				Events = isolated
			};
			
			QueueInsert(mainQueue, batch);
			BatchUnpacker.RegisterBatchFlag<BatchedLogicEvent<T>, T>();
		}
		
		protected void BatchState(IList<ILogicEvent> mainQueue)
		{
			var isolated = IsolateFrom<ChangeCardsState>(mainQueue);
			var groups = isolated.GroupBy(x => (x.OldState, x.NewState));
			var batch = groups.Select(group => (ILogicEvent)new ChangeCardsState
			{
				OldState = group.Key.OldState,
				NewState = group.Key.NewState,
				CardIds = group.SelectMany(data => data.CardIds).Distinct().ToList(),
				Order = group.Min(x => x.Order)
			}).ToArray();

			QueueInsert(mainQueue, batch);
		}

		protected static IEnumerable<T> IsolateFrom<T>(IList<ILogicEvent> mainQueue) where T : ILogicEvent
		{
			if (mainQueue.IsNullOrEmpty())
				return Array.Empty<T>();
			
			var isolated = mainQueue.OfType<T>().ToArray();
			isolated.ForEach(x => mainQueue.Remove(x));
			return isolated;
		}

		protected static void QueueInsert(IList<ILogicEvent> mainQueue, params ILogicEvent[] events)
		{
			if (events.IsNullOrEmpty())
				return;

			mainQueue.AddRange(events.Where(x => x != null));
		}
	}
}
