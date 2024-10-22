using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using RR.Core.DebugSystem;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

namespace BerserkV3.Common.AnalyticsSystem
{
	public class UnityAnalyticProvider : AnalyticsProviderBase
	{
		protected override async UniTask OnInitAsync(CancellationToken token)
		{
			try
			{
				AnalyticsService.Instance.StartDataCollection();
			}
			catch (Exception ex)
			{
				RRLogger.Error($"Error during initialization {typeof(UnityAnalyticProvider)}: {ex.Message}");
			}
		}

		protected override void OnSendThroughSdk(string eventName, Dictionary<string, object> customData)
		{
			/*
			 * If we need to use Unity Analytics,
			 * then we need to optimize the events for these parameters,
			 * or contact support to increase these parameters:
			 *
			 * Default limit of 10 parameters per custom event
			 * Default limit of 500 characters for the dictionary content
			 * Default limit of 100 custom events per hour, per user
			 */
			
			base.OnSendThroughSdk(eventName, customData);
			AnalyticsService.Instance.CustomData(eventName, customData);
		}

		protected override void OnDisposed()
		{
			AnalyticsService.Instance.StopDataCollection();
		}
	}
}