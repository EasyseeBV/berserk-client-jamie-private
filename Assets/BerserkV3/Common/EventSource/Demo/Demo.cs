using System;
using System.Threading.Tasks;
using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.Common.Utils;
using Zenject;

namespace BerserkV3.Common.EventSource.Demo
{
	public readonly struct DemoEvent
	{
		public string Message { get; }

		public DemoEvent(string message)
		{
			Message = message;
		}
	}

	public class DemoSubscriber : IInitializable, IDisposable
	{
		private readonly IEventsSource eventsSource;
		private IDisposables subscriptions;

		public DemoSubscriber(IEventsSource eventsSource)
		{
			this.eventsSource = eventsSource;
		}
		
		public void Dispose()
		{
			subscriptions?.Dispose();
			subscriptions = null;
		}

		public void Initialize()
		{
			eventsSource.Subscribe<DemoEvent>(OnDemoInvokedAction).AddTo(ref subscriptions);
			eventsSource.Subscribe<DemoEvent>(OnDemoInvokedActionT).AddTo(subscriptions);
			eventsSource.Subscribe<DemoEvent>(OnDemoInvokedFuncTAsync).AddTo(subscriptions);
		}
		
		private void OnDemoInvokedAction() => DefaultSharedLogger.Log($"Some one published the event : {nameof(DemoEvent)}");
		private void OnDemoInvokedActionT(DemoEvent data) => DefaultSharedLogger.Log(data.Message);
		private Task OnDemoInvokedFuncTAsync(DemoEvent data)
		{
			DefaultSharedLogger.Log(data.Message);
			return Task.CompletedTask;
		}
	}

	public class DemoPublisher : IInitializable
	{
		private readonly IEventPublisher eventPublisher;
		
		public DemoPublisher(IEventPublisher eventPublisher)
		{
			this.eventPublisher = eventPublisher;
		}

		public async void Initialize()
		{
			eventPublisher.Publish(new DemoEvent("Simple publish."));
			await eventPublisher.PublishAsync(new DemoEvent("Queued async publish."));
			await eventPublisher.PublishParallelAsync(new DemoEvent("Parallel async publish."));
		}
	}
}