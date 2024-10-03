using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.LogicContext
{
	public class SharedEventSource : ISharedEventsSource
	{
		private class Subscriber : IDisposable, IComparable<Subscriber>
		{
			public Delegate Callback;
			public event Action OnDisposeAction;
			public bool HasParameters { get; set; }
			public int Order { get; set; }

			public void Dispose()
			{
				if (OnDisposeAction == null) // prevent dead lock
					return;
				
				var memo = OnDisposeAction;
				Callback = null;
				OnDisposeAction = null;
				memo?.Invoke();
			}

			public int CompareTo(Subscriber other)
			{
				if (ReferenceEquals(this, other)) return 0;
				if (ReferenceEquals(null, other)) return 1;
				return Order.CompareTo(other.Order);
			}
		}
		
		private readonly Dictionary<Type, List<Subscriber>> subscribers = new();
		private bool sortRequested;

		public IDisposable Subscribe<T>(Action<T> callback, CancellationToken? token = null, int? order = null)
		{
			return InternalRegisterSubscriber<T>(callback, true, order, token);
		}

		public IDisposable Subscribe<T>(Action callback, CancellationToken? token = null, int? order = null)
		{
			return InternalRegisterSubscriber<T>(callback, false, order, token);
		}

		public IDisposable Subscribe<T>(Func<Task> callback, CancellationToken? token = null, int? order = null)
		{
			return InternalRegisterSubscriber<T>(callback, false, order, token);
		}

		public IDisposable Subscribe<T>(Func<T, Task> callback, CancellationToken? token = null, int? order = null)
		{
			return InternalRegisterSubscriber<T>(callback, true, order, token);
		}

		public void Dispose()
		{
			foreach (var listeners in subscribers.Values.ToArray())
				listeners.ToList().ForEach(x => x?.Dispose());
			
			subscribers.Clear();
		}

		public void Publish<T>(T value)
		{
			if (!subscribers.TryGetValue(typeof(T), out var listOfListeners))
				return;

			if (sortRequested) // just for optimize
			{
				listOfListeners.Sort();
				sortRequested = false;
			}
			
			foreach (var subscriber in listOfListeners.ToArray())
			{
				try
				{
					if (subscriber.HasParameters)
					{
						subscriber.Callback?.DynamicInvoke(value);
						continue;
					}
				
					subscriber.Callback?.DynamicInvoke();
				}
				catch (Exception e)
				{
					DefaultSharedLogger.Error($"[{GetType().Name}] Some exception caused when {nameof(Publish)} event method : {subscriber.Callback?.Method.Name}, with registered type : {typeof(T).FullName}. Full exception: {e}");
				}
			}
		}
		
		private IDisposable InternalRegisterSubscriber<T>(Delegate callback, bool hasParameters, int? order, CancellationToken? token)
		{
			var registerType = typeof(T);
			if (!subscribers.TryGetValue(registerType, out var registered))
				subscribers[registerType] = registered = new List<Subscriber>();
			
			var subscriber = new Subscriber
			{
				Callback = callback, 
				Order = order ?? registered.Count,
				HasParameters = hasParameters,
			};

			var cancellationTokenRegistration = token?.Register(subscriber.Dispose);

			subscriber.OnDisposeAction += () =>
			{
				if (cancellationTokenRegistration is {Token: {IsCancellationRequested: false}}) // registered and disposed not by token
					cancellationTokenRegistration.Value.Dispose(); // then release
				
				if (!subscribers.TryGetValue(registerType, out var result))
					return;

				result.Remove(subscriber);
				if (result.Count == 0)
					subscribers.Remove(registerType);
			};
			
			registered.Add(subscriber);
			sortRequested = true;
			return subscriber;
		}
	}
}