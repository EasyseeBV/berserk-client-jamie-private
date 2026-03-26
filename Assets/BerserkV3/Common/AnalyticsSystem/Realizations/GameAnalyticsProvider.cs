using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameAnalyticsSDK;
using RR.Core.DebugSystem;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BerserkV3.Common.AnalyticsSystem
{
	public class GameAnalyticsProvider : AnalyticsProviderBase, IGameAnalyticsATTListener
	{
		private const string GAME_ANALYTICS_PREFAB_PATH = "GameAnalyticsPrefab";
		
		private bool isInitialized;
		
		public GameAnalyticsProvider()
		{
			var loadedAnalyticsGameObject = Resources.Load(GAME_ANALYTICS_PREFAB_PATH);
			if (loadedAnalyticsGameObject == null)
			{
				RRLogger.Error($"{typeof(GameAnalyticsProvider)} missing Resources/{GAME_ANALYTICS_PREFAB_PATH}. Analytics init will be skipped.");
				return;
			}

			var analyticsGameObject = Object.Instantiate(loadedAnalyticsGameObject);
			Object.DontDestroyOnLoad(analyticsGameObject);
		}
		protected override UniTask OnInitAsync(CancellationToken token)
		{
			// can't init before user id is setted
			return UniTask.CompletedTask;
		}


		protected override void OnSetUserId(string userId) // Set id must be performed before Initialization;
		{
			if (isInitialized)
				return;
			
			GameAnalytics.SetCustomId(userId);

			try
			{
				RRLogger.Log($"{typeof(GameAnalyticsProvider)} Initializing.");
				if (Application.platform == RuntimePlatform.IPhonePlayer)
				{
					GameAnalytics.RequestTrackingAuthorization(this);
				}
				else
				{
					InitAnalytics();
				}
			}
			catch (Exception ex)
			{
				RRLogger.Error($"Error during initialization {typeof(GameAnalyticsProvider)}: {ex.Message}");
			}
		}

		protected override void OnSendThroughSdk(string eventName, Dictionary<string, object> customData)
		{
			if (isInitialized)
				return;
			
			GameAnalytics.NewDesignEvent(eventName, customData);
		}

		public void GameAnalyticsATTListenerNotDetermined()
		{
			RRLogger.Log($"{typeof(GameAnalyticsProvider)} ATT Not Determined.");
			InitAnalytics();
		}

		public void GameAnalyticsATTListenerRestricted()
		{
			RRLogger.Log($"{typeof(GameAnalyticsProvider)} ATT Restricted.");
			InitAnalytics();
		}

		public void GameAnalyticsATTListenerDenied()
		{
			RRLogger.Log($"{typeof(GameAnalyticsProvider)} ATT Denied.");
			InitAnalytics();
		}

		public void GameAnalyticsATTListenerAuthorized()
		{
			RRLogger.Log($"{typeof(GameAnalyticsProvider)} ATT Authorized.");
			InitAnalytics();
		}

		private void InitAnalytics()
		{
			GameAnalytics.onInitialize += OnInitialized;
			GameAnalytics.Initialize();
		}

		private void OnInitialized(object sender, bool state)
		{
			GameAnalytics.onInitialize -= OnInitialized;

			if (state)
			{
				isInitialized = true;
				RRLogger.Log($"{typeof(GameAnalyticsProvider)} Initialization successful.");
			}
			else
			{
				RRLogger.Log($"{typeof(GameAnalyticsProvider)} Initialization unsuccessful.");
			}
		}
	}
}
