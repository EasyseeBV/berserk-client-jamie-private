using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using RR.Core.DebugSystem;
using RR.Core.Extensions;

namespace BerserkV3.Common.AnalyticsSystem
{
	public abstract class AnalyticsProviderBase : IAnalyticProvider, IDisposable
	{
		private CancellationTokenSource cancellationTokenSource;
		private readonly Queue<(string, Dictionary<string, object>)> eventQueue = new();
		protected bool Initialized { get; private set; }

		public async UniTask InitAsync(CancellationToken token)
		{
			try
			{			
				if (Initialized)
					return;
				
				await OnInitAsync(token);
				Initialized = true;
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
				Dispose();
			}
		}

		public void Dispose()
		{
			if (!Initialized)
				return;
			
			OnDisposed();
			cancellationTokenSource?.Cancel();
			cancellationTokenSource?.Dispose();
			cancellationTokenSource = null;
			Initialized = false;
		}

		public void AddToSendQueue(string eventName, Dictionary<string, object> customData)
		{
			if (!Initialized)
				return;
			
			OnAddToSendQueue(eventName, customData);
		}

		public void SetUpAbGroups()
		{
			if (!Initialized)
				return;
			
			try
			{			
				OnSetUpAbGroups();
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		public void SetUserId(string userId)
		{
			if (!Initialized)
				return;

			OnSetUserId(userId);
		}
		
		protected abstract UniTask OnInitAsync(CancellationToken token);
		
		protected virtual void OnAddToSendQueue(string eventName, Dictionary<string, object> customData)
		{
			var isRunning = cancellationTokenSource != null;
			eventQueue.Enqueue((eventName, customData));
			if (isRunning || !Initialized)
				return;

			cancellationTokenSource = new CancellationTokenSource();
			ProcessQueueAsync(cancellationTokenSource.Token).Forget();
		}
		
		protected virtual void OnSetUpAbGroups(){}
		
		protected virtual void OnSetUserId(string userId){}
		
		protected virtual void OnSendThroughSdk(string eventName, Dictionary<string, object> customData)
		{
			RRLogger.Log($"[{GetType().Name}] {nameof(OnSendThroughSdk)} Event: {eventName}; params: {customData.JoinToString()}");
		}

		private async UniTask ProcessQueueAsync(CancellationToken cancellationToken)
		{
			if (!Initialized)
				return;
			
			while (!cancellationToken.IsCancellationRequested && eventQueue.Count > 0)
			{
				await UniTask.Yield();
				var analyticsEvent = eventQueue.Dequeue();
				OnSendThroughSdk(analyticsEvent.Item1, analyticsEvent.Item2);
			}

			cancellationTokenSource?.Dispose();
			cancellationTokenSource = null;
		}
		
		protected virtual void OnDisposed(){}
	}
}