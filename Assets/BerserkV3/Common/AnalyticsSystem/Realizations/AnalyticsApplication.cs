using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Berserk.Shared.Data.Identity;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Lobby.Network;
using BerserkV3.Startup.Utils;
using Cysharp.Threading.Tasks;
using RR.Core.DebugSystem;
using RR.Core.Extensions;

namespace BerserkV3.Common.AnalyticsSystem
{

	public class AnalyticsApplication : IBerserkAnalyticsApplication, IDisposable
	{
		private readonly IEnumerable<IAnalyticProvider> analyticProviders;
		private readonly IEnumerable<IPermissionService> premissionServices;
		public UserAnalytics AnalyticsData { get; private set; } = new();

		public AnalyticsApplication(
			IEnumerable<IAnalyticProvider> analyticProviders,
			IEnumerable<IPermissionService> premissionServices)
		{
			this.analyticProviders = analyticProviders;
			this.premissionServices = premissionServices;
		}

		public void SetUserId(string userId)
		{
			analyticProviders.ForEach(provider => provider.SetUserId(userId));
		}

		public void InitAbGroup()
		{
			analyticProviders.ForEach(provider => provider.SetUpAbGroups());
		}

		public async UniTask InitAsync(CancellationToken token)
		{
			await UniTask.WhenAll(premissionServices.Select(x => x.RequestPermissionAsync(token))).AttachExternalCancellation(token);
			await UniTask.WhenAll(analyticProviders.Select(x => x.InitAsync(token))).AttachExternalCancellation(token);
			InitAbGroup();
			AnalyticsBus.SendCustomEvent.SubscribeRaw(SendModel);
			AnalyticsBus.SendDelayedEvents.SubscribeRaw(RefreshAndSendDelayedEventsAsync);
		}

		private async void RefreshAndSendDelayedEventsAsync()
		{
			if (AnalyticsData.FirstDeckCreatedSent
			    && AnalyticsData.FirstGameLostSent
			    && AnalyticsData.FirstGameWonSent)
				return;

			try
			{
				await TaskUtil.RetryAsync(TryGetAnalyticsAsync);
				await SendDelayedEvents();
			}
			catch (Exception e)
			{
				RRLogger.Error($"[{GetType().Name.Orange()}] Can't fetch an analytics data: {e}");
			}

			async Task<TryResult> TryGetAnalyticsAsync()
			{
				var result = await PlayerAPI.GetAnalyticsAsync();
				if (result.IsSuccess && result.Data != null)
					AnalyticsData = result.Data;
				return !result.IsSuccess ? TryResult.Retry : TryResult.Success;
			}
		}

		public void Dispose()
		{
			AnalyticsBus.SendCustomEvent.Unsubscribe(SendModel);
			AnalyticsBus.SendDelayedEvents.Unsubscribe(RefreshAndSendDelayedEventsAsync);
		}

		private void SendModel(IAnalyticsModel model)
		{
			var parameters = model.ToDictionary();
			QueueEventSend(model.Key, parameters);
		}

		private void QueueEventSend(string eventName, Dictionary<string, object> customData)
		{
			Clear();
			analyticProviders.ForEach(provider => provider.AddToSendQueue(eventName, customData));
			RRLogger.Log($"{nameof(QueueEventSend)} {eventName}");

			void Clear()
			{
				var removedDataIds = customData.Keys.ToArray();
				removedDataIds.ForEach(id =>
				{
					if (customData[id] == null)
						customData.Remove(id);
				});
			}
		}

		private async UniTask SendDelayedEvents()
		{
			var anyChages = false;
			if (!AnalyticsData.FirstGameWonSent)
			{
				var eventSent = SendCustomEvent(AnalyticsData.FirstGameWon, nameof(AnalyticsData.FirstGameWon));
				anyChages |= eventSent;
				AnalyticsData.FirstGameWonSent = eventSent;
			}
			
			if (!AnalyticsData.FirstGameLostSent)
			{
				var eventSent = SendCustomEvent(AnalyticsData.FirstGameLost, nameof(AnalyticsData.FirstGameLost));
				anyChages |= eventSent;
				AnalyticsData.FirstGameLostSent = eventSent;
			}
			
			if (!AnalyticsData.FirstDeckCreatedSent)
			{
				var eventSent = SendCustomEvent(AnalyticsData.FirstDeckCreated, nameof(AnalyticsData.FirstDeckCreated));
				anyChages |= eventSent;
				AnalyticsData.FirstDeckCreatedSent = eventSent;
			}
			if (anyChages)
				await PlayerAPI.PostAnalytics(AnalyticsData);
		}

		private bool SendCustomEvent(string cutomData, string eventName)
		{
			try
			{
				if (string.IsNullOrEmpty(cutomData))
					return false;

				var parameters = cutomData.ParseCustomData();
				if (parameters.Count == 0)
					return false;

				SendModel(new CustomModel(eventName, parameters));
				return true;
			}
			catch (Exception e)
			{
				RRLogger.Error($"[{GetType().Name.Orange()}] Can't send a deferred event : {e}");
				return false;
			}
		}
	}

}