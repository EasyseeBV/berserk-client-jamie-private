using System;
using System.Collections.Generic;
using System.Threading;
using AppsFlyerSDK;
using Cysharp.Threading.Tasks;
using RR.Core.DebugSystem;

namespace BerserkV3.Common.AnalyticsSystem
{

	public class AppsFlyerAnalyticProvider : AnalyticsProviderBase
	{
#if UNITY_IOS
		private const string DEV_KEY = "7oA8eo9vf3wbKFPdGNEAjQ";
		private const string APP_ID = "1566367597";
#else
		private const string DEV_KEY = "yMudmLNRpdH2AemXsob6sj";
		private const string APP_ID = null;
#endif

		protected override UniTask OnInitAsync(CancellationToken token)
		{
			try
			{
				AppsFlyer.OnRequestResponse += OnAppsFlyerRequestResponse;
				AppsFlyer.OnInAppResponse += OnAppsFlyerInAppResponse;
				AppsFlyer.initSDK(DEV_KEY, APP_ID, null);
#if DEBUG
				AppsFlyer.setIsDebug(true);
#else
				AppsFlyer.setIsDebug(false);
#endif

#if UNITY_IOS && !UNITY_EDITOR
				AppsFlyer.waitForATTUserAuthorizationWithTimeoutInterval(60);
#endif
				AppsFlyer.startSDK();
				RRLogger.Log($"{nameof(AppsFlyer)} sdk {AppsFlyer.getSdkVersion()} started");
				return UniTask.CompletedTask;
			}
			catch
			{
				AppsFlyer.OnRequestResponse -= OnAppsFlyerRequestResponse;
				AppsFlyer.OnInAppResponse -= OnAppsFlyerInAppResponse;
				throw;
			}
		}

		private void OnAppsFlyerInAppResponse(object sender, EventArgs e)
		{
			if (!Initialized)
				return;
			
			var args = e as AppsFlyerRequestEventArgs;
			if (args == null)
				RRLogger.Error($"{nameof(AppsFlyer)} missing args");

			var code = args?.statusCode ?? 0;
			AppsFlyer.AFLog("AppsFlyerInAppResponse", " status code " + code);
			RRLogger.Log($"{nameof(AppsFlyer)} AppsFlyerInAppResponse status code: {code}");
			if (!string.IsNullOrEmpty(args?.errorDescription))
				RRLogger.Error($"{nameof(AppsFlyer)} AppsFlyerInAppResponse error: {args.errorDescription}");
		}

		private void OnAppsFlyerRequestResponse(object sender, EventArgs e)
		{
			if (!Initialized)
				return;
			
			var args = e as AppsFlyerRequestEventArgs;
			if (args == null)
				RRLogger.Error($"{nameof(AppsFlyer)} missing args");

			var code = args?.statusCode ?? 0;
			AppsFlyer.AFLog("AppsFlyerOnRequestResponse", " status code " + code);
			RRLogger.Log($"{nameof(AppsFlyer)} AppsFlyerOnRequestResponse status code: {code}");
			if (!string.IsNullOrEmpty(args?.errorDescription))
				RRLogger.Error($"{nameof(AppsFlyer)} AppsFlyerOnRequestResponse error: {args.errorDescription}");
		}

		protected override void OnSendThroughSdk(string eventName, Dictionary<string, object> customData)
		{
			base.OnSendThroughSdk(eventName, customData);
			var eventData = ParseEventData(eventName, customData);
			AppsFlyer.sendEvent(eventData.Item1, eventData.Item2);
		}

		private (string, Dictionary<string, string>) ParseEventData(string eventName, Dictionary<string, object> customData)
		{
			var data = new Dictionary<string, string>();
			foreach (var (key, value) in customData)
				data.Add(key, value.ToString());

			if (eventName == EventHelper.LOGIN)
				eventName = AFInAppEvents.LOGIN;

			if (eventName == EventHelper.TUTORIAL_COMPLETED)
				eventName = AFInAppEvents.TUTORIAL_COMPLETION;

			return (eventName, data);
		}

		protected override void OnSetUserId(string userId)
		{
			AppsFlyer.setCustomerUserId(userId);
		}

		protected override void OnDisposed()
		{
			AppsFlyer.OnRequestResponse -= OnAppsFlyerRequestResponse;
			AppsFlyer.OnInAppResponse -= OnAppsFlyerInAppResponse;
		}
	}

}