using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.LogicEvents;
using RR.Core.Extensions;
using Sirenix.Utilities;

namespace BerserkV3.GameCore.LogicEventsProcessor.BatchSystem
{
	internal class AoeBatcher : BatcherBase
	{
		public AoeBatcher(IBatchUnpacker batchUnpacker) : base(batchUnpacker)
		{
		}

		public override bool ShouldPerformBatching(IList<ILogicEvent> mainQueue)
		{
			return mainQueue.Any(x => x is BatchEvent);
		}

		public override void Process(IList<ILogicEvent> mainQueue)
		{
			DefaultSharedLogger.Log($"[{GetType().Name.Orange()}] Process Batching {"Start".Green()}");
			var newQueue = ExtractBatchQueue(mainQueue);
			if (newQueue.IsNullOrEmpty()) 
				return;
			
			foreach (var queue in newQueue)
			{
				Batch<ChangeObjectHp>(queue, false);
				Batch<ChangeObjectAttack>(queue, false);
				Batch<ChangeObjectMana>(queue, false);
				Batch<ChangeObjectArmor>(queue, false);
				Batch<ChangeObjectAbilityMoves>(queue, false);
				Batch<ChangeObjectMoves>(queue, false);
				Batch<ChangeCardPosition>(queue);
				Batch<CreateObject>(queue);
				BatchState(queue);
				Batch<DeleteObjectEffect>(queue, false);
				Batch<AddObjectEffect>(queue);
				QueueInsert(mainQueue, queue.ToArray());
			}

			DefaultSharedLogger.Log($"[{GetType().Name.Orange()}] Process Batching {"Ended".Green()}");
		}

		private IList<IList<ILogicEvent>> ExtractBatchQueue(ICollection<ILogicEvent> mainQueue)
		{
			var batches = new List<int>();
			var newQueues = new Dictionary<int, IList<ILogicEvent>>();
			foreach (var logicEvent in mainQueue.ToArray())
			{
				if (batches.Count == 0 && logicEvent is not BatchEvent)
					continue;

				if (logicEvent is BatchEvent batchEvent)
				{
					DefaultSharedLogger.Log($"[{GetType().Name.Orange()}]  {(batchEvent.Start? "Started" : "Ended").Green()}, Batch Id : {batchEvent.Id}");
					mainQueue.Remove(logicEvent);

					if (batchEvent.Start && !batches.Contains(batchEvent.Id))
					{
						if (!newQueues.ContainsKey(batchEvent.Id))
							newQueues.Add(batchEvent.Id, new List<ILogicEvent>());
						
						batches.Add(batchEvent.Id);
					}

					if (batchEvent.End)
						batches.Remove(batchEvent.Id);
						
					continue;
				}

				if (batches.Count <= 0 || !newQueues.TryGetValue(batches[^1], out var batchQueue)) 
					continue;
				
				batchQueue.Add(logicEvent);
				mainQueue.Remove(logicEvent);
			}

			return newQueues.Values.ToList();
		}
	}
}
