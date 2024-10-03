using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicEvents;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.LogicEventsProcessor.BatchSystem;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using Zenject;

namespace BerserkV3.GameCore.LogicEventsProcessor
{
	public interface IGameLogicEventsSource
	{
		void Subscribe<T>(Action func, CancellationToken token = default, int order = 0) where T : ILogicEvent;
		void Subscribe<T>(Action<T> func, CancellationToken token = default, int order = 0) where T : ILogicEvent;
		void Subscribe<T>(Func<T, UniTask> func, CancellationToken token = default, int order = 0) where T : ILogicEvent;
	}

	public interface IGameLogicEventsProcessor
	{
		void Process(params ILogicEvent[] events);
		UniTask ProcessUnQueueAsync(ILogicEvent logicEvent);
	}

	public class GameLogicEventsProcessor : DisposableWithCts, IGameLogicEventsSource, IGameLogicEventsProcessor
	{
		private struct Sunscriber : IComparable, IComparable<Sunscriber>
		{
			public Func<ILogicEvent, UniTask> Callback;
			public int Order;

			public int CompareTo(object obj)
			{
				if (obj is not Sunscriber other)
					return -1;
				
				return CompareTo(other);
			}

			public int CompareTo(Sunscriber other)
			{
				return Order.CompareTo(other.Order);
			}

			public override bool Equals(object obj)
			{
				return obj is Sunscriber other && other.Callback == Callback && Order == other.Order;
			}

			public bool Equals(Sunscriber other)
			{
				return Equals(Callback, other.Callback) && Order == other.Order;
			}

			public override int GetHashCode()
			{
				return HashCode.Combine(Callback, Order);
			}
		}
		private readonly Channel<ILogicEvent> logicChannel;
		private readonly Dictionary<Type, List<Sunscriber>> subscribersMap;
		private readonly IVisibleLocalState visibleLocalState;
		private readonly ILogicEventsBatcher logicEventsBatcher;

		[Inject]
		public GameLogicEventsProcessor(
			IVisibleLocalState visibleLocalState, 
			ILogicEventsBatcher logicEventsBatcher)
		{
			subscribersMap = new Dictionary<Type, List<Sunscriber>>();
			logicChannel = Channel.CreateSingleConsumerUnbounded<ILogicEvent>();
			this.visibleLocalState = visibleLocalState;
			this.logicEventsBatcher = logicEventsBatcher;

			logicChannel.Reader
				.ReadAllAsync(Token)
				.SubscribeAwait(ProcessUnQueueAsync, Token);
		}

		public void Process(params ILogicEvent[] events)
		{
			if (events == null || events.Length == 0)
				return;

			var eventList = events.ToList();

			logicEventsBatcher.Process(eventList);

			foreach (var logicEvent in eventList)
				logicChannel.Writer.TryWrite(logicEvent);
		}

		public async UniTask ProcessUnQueueAsync(ILogicEvent logicEvent)
		{
			try
			{
				switch (logicEvent)
				{
					case InitializeGame initializeGame:
						var message = initializeGame.ReInitialize
							? "_____RE_INITIALIZE_GAME____"
							: "______INITIALIZE_GAME______";
						RRLogger.Log(message.Green() + $"{logicEvent}");
						break;
					default:
						RRLogger.Log($"{nameof(ProcessUnQueueAsync)} - {logicEvent}");
						break;
				}

				var unpackedEvents = logicEventsBatcher.UnpackBatch(logicEvent);

				await UniTask.WhenAll(unpackedEvents.Select(ProcessSingleLogicEventAsync).ToArray());
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		private async UniTask ProcessSingleLogicEventAsync(ILogicEvent logicEvent)
		{
			visibleLocalState.ApplyEvent(logicEvent);

			// in current app state can be created new subscribers then will be changed subscribers
			// subscribers.ToArray() need copy app state of subscribers
			if (subscribersMap.TryGetValue(logicEvent.GetType(), out var subscribers))
				await UniTask.WhenAll(subscribers.ToArray().Select(listener => listener.Callback.Invoke(logicEvent)));
		}

		public void Subscribe<T>(Action func, CancellationToken token = default, int order = 0) where T : ILogicEvent
		{
			Subscribe<T>(_ => func?.Invoke(), token, order);
		}

		public void Subscribe<T>(Action<T> func, CancellationToken token, int order = 0) where T : ILogicEvent
		{
			Subscribe<T>(logicEvent =>
			{
				func?.Invoke(logicEvent);
				return UniTask.CompletedTask;
			}, token, order);
		}

		public void Subscribe<T>(Func<T, UniTask> func, CancellationToken token, int order = 0) where T : ILogicEvent
		{
			var type = typeof(T);
			if (!subscribersMap.TryGetValue(type, out var subscribers))
			{
				subscribers = new List<Sunscriber>();
				subscribersMap.Add(type, subscribers);
			}

			var sunscriber = new Sunscriber {Callback = FuncLocal, Order = order};
			token.Register(() => subscribers.Remove(sunscriber));
			subscribers.Add(sunscriber);
			subscribers.Sort();
			
			async UniTask FuncLocal(ILogicEvent logicEvent)
			{
				var task = func?.Invoke((T)logicEvent) ?? UniTask.CompletedTask;
				await task.AttachExternalCancellation(token)
					.AttachExternalCancellation(Token)
					.SuppressCancellationThrow();
			}
		}

		public override void Dispose()
		{
			base.Dispose();

			logicChannel.Writer.TryComplete();
			subscribersMap.Clear();
		}
	}
}