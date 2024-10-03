using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.Controllers
{
	public readonly struct QueueSentEvent : ISharedEvent
	{
		public string UserName { get; }
		public List<ILogicEvent> Queue { get; }

		public QueueSentEvent(string userName, List<ILogicEvent> queue)
		{
			UserName = userName;
			Queue = queue;
			var lastOrder = int.MinValue;
			foreach (var logicEvent in Queue)
			{
				if (lastOrder <= logicEvent.Order)
				{
					lastOrder = logicEvent.Order;
					continue;
				}

				DefaultSharedLogger.Error($"Queue has invalid order : {string.Join("/n", Queue)}");
			}
		}
	}

	public class LogicQueueController : ILogicQueueController
	{
		private readonly IGameContext gameContext;
		private Dictionary<string, Queue<ILogicEvent>> LogicQueue { get; }

		public LogicQueueController(IGameContext gameContext)
		{
			this.gameContext = gameContext;
			LogicQueue = new Dictionary<string, Queue<ILogicEvent>>();
		}

		public void Initialize()
		{
			foreach (var player in gameContext.PlayerRepository)
				LogicQueue.Add(player.UserId, new Queue<ILogicEvent>());
		}

		public void Add(ILogicEvent logicEvent, string userId = null)
		{
			if (LogicQueue.Count == 0)
			{
				DefaultSharedLogger.Error($"You are trying to add events, but no queue is initialized.. UserId: {userId}, EventToAdd: {logicEvent}");
				return;
			}

			logicEvent.Order = gameContext.OrderGenerator.Next();

			if (string.IsNullOrEmpty(userId))
			{
				foreach (var (_, queue) in LogicQueue)
					queue.Enqueue(logicEvent);

				return;
			}

			if (LogicQueue.TryGetValue(userId, out var userQueue))
				userQueue.Enqueue(logicEvent);
		}

		public void SendAndClearLogicQueue()
		{
			foreach (var (userId, queue) in LogicQueue)
			{
				var memo = queue.ToList();
				queue.Clear();

				if (memo.Count > 0 && gameContext.PlayerRepository.TryGet(userId, out var player))
					gameContext.SharedEventsSource.Publish(new QueueSentEvent(player.RuntimeData.UserName, memo));
			}
		}

		public void Dispose()
		{
			foreach (var (userId, queue) in LogicQueue)
			{
				if (queue.Count > 0)
					DefaultSharedLogger.Error($"[{GetType().Name}] Queue for user: {userId} was disposed with events!\n{string.Join("\n==========", queue.Select(ev => ev.ToString()))}");
			}
			LogicQueue.Clear();
		}
	}
}