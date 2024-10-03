using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BerserkV3.Common.Network;
using Cysharp.Threading.Tasks;
using Firebase.Analytics;
using RR.Core.DebugSystem;
using RR.Core.Extensions;

namespace BerserkV3.Common.AnalyticsSystem
{
	public class FirebaseAnalyticProvider : AnalyticsProviderBase
	{
		protected override UniTask OnInitAsync(CancellationToken token)
		{
			var environment = EnvironmentSwitcher.CurrentEnvironment.ToString();
			FirebaseAnalytics.SetUserProperty(nameof(environment).ToTitleCase(), environment);
			FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
			RRLogger.Log($"{nameof(FirebaseAnalytics)} enable");
			return UniTask.CompletedTask;
		}

		protected override void OnSetUserId(string userId)
		{
			FirebaseAnalytics.SetUserId(userId);
		}

		protected override void OnSendThroughSdk(string eventName, Dictionary<string, object> customData)
		{
			base.OnSendThroughSdk(eventName, customData);
			var firebaseParams = customData
				.Select(data => new Parameter(data.Key, data.Value.ToString()))
				.ToArray();
			FirebaseAnalytics.LogEvent(eventName, firebaseParams);
		}
	}
}