using System;
using System.Collections.Generic;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicEvents;

namespace BerserkV3.GameCore.LogicEventsProcessor.BatchSystem
{
	public class BatchUnpacker : IBatchUnpacker
	{
		private readonly Dictionary<Type, Func<ILogicEvent, IEnumerable<ILogicEvent>>> usedBatchFlags = new();

		public IEnumerable<ILogicEvent> UnpackBatch(ILogicEvent batchedLogicEvent)
		{
			var type = batchedLogicEvent.GetType();

			if (usedBatchFlags.ContainsKey(type))
			{
				foreach (var logicEvent in usedBatchFlags[type](batchedLogicEvent))
					yield return logicEvent;

				yield break;
			}

			yield return batchedLogicEvent;
		}

		public void RegisterBatchFlag<TBatch, TEvent>() where TBatch : BatchedLogicEvent<TEvent> where TEvent : ILogicEvent
		{
			usedBatchFlags[typeof(TBatch)] = GetEvents<TEvent>;
		}

		private IEnumerable<ILogicEvent> GetEvents<TEvent>(ILogicEvent logicEvent) where TEvent : ILogicEvent
		{
			if (logicEvent is BatchedLogicEvent<TEvent> batchedEvent)
			{
				foreach (var e in batchedEvent.Events)
					yield return e;

				yield break;
			}

			yield return logicEvent;
		}
	}
}
