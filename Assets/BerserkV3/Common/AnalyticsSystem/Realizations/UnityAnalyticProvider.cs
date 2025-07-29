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
				await UnityServices.InitializeAsync();
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
			//TODO: needs to be tested
			base.OnSendThroughSdk(eventName, customData);
			var evt = new CustomEvent(eventName);

			if (customData != null)
			{
				foreach (var kvp in customData)
				{
					try
					{
						evt.Add(kvp.Key, kvp.Value);
					}
					catch (ArgumentException ex)
					{
						UnityEngine.Debug.LogWarning($"Analytics parameter skipped: {kvp.Key} — unsupported type {kvp.Value?.GetType()?.Name}");
					}
				}
			}

			AnalyticsService.Instance.RecordEvent(evt);
		}

		protected override void OnDisposed()
		{
			AnalyticsService.Instance.StopDataCollection();
		}
	}
}