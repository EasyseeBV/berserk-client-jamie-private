using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BerserkV3.Common.Network;
using BerserkV3.Common.Utils;
using BerserkV3.Startup.Abstractions;
using BerserkV3.Startup.Network.Enums;
using BerserkV3.Startup.Utils;
using BestHTTP;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using UnityEngine;

namespace BerserkV3.Startup.Network
{
	public static class ServerRouter
	{
		public static async Task<TryResult> SelectLowestLatencyServer(IMessageApplication messageApplication)
		{
			var tuples = new List<(Region, string)>();

			foreach (var (key, value) in URLs.URLS_BY_REGION)
				tuples.Add((key, value.GetServerURL(EnvironmentSwitcher.CurrentEnvironment)));

			var lowestLatencyRegion = await GetLowestLatencyUrl(tuples).AddLoadingTask();

			if (!lowestLatencyRegion.HasValue)
			{
				await messageApplication.Info("The timeout has exceeded, check your internet connection and try again or try again later.");
				return TryResult.Retry;
			}
			
			EnvironmentSwitcher.SwitchRegion(lowestLatencyRegion.Value);
			return TryResult.Success;
		}

		private static async Task<Region?> GetLowestLatencyUrl(List<(Region, string)> urls)
		{
			if (urls == null || urls.Count == 0)
				return default;

			var minLatency = int.MaxValue;
			Region? lowestLatencyRegion = null;

			foreach (var (region, url) in urls)
			{
				if (url == null)
					continue;

				var latency = await Ping(url);
				var pingValue = latency.HasValue ? latency.ToString() : "Unreachable";
				RRLogger.Log($"[{nameof(ServerRouter).Orange()}] {region} - {url} ping is {pingValue}");

				if (latency < minLatency)
				{
					minLatency = latency.Value;
					lowestLatencyRegion = region;
				}
			}

			if (lowestLatencyRegion.HasValue)
				RRLogger.Log($"[{nameof(ServerRouter).Orange()}] Lowest latency url is: {lowestLatencyRegion}");

			return lowestLatencyRegion;
		}

		private static async Task<int?> Ping(string url, int retry = 3)
		{
			var uri = new Uri($"{url}/{URLs.API_SUFFIX}/health");
			var tryCount = 0;

			while (Application.isPlaying && tryCount < retry)
			{
				try
				{
					var request = new HTTPRequest(uri);
					var requestStartTime = Time.realtimeSinceStartupAsDouble;
					request.Timeout = TimeSpan.FromSeconds(5f);
					var response = await request.GetHTTPResponseAsync();

					if (response.IsSuccess) 
						return (int)TimeSpan.FromSeconds(Time.realtimeSinceStartupAsDouble - requestStartTime).TotalMilliseconds;
				}
				catch (Exception e)
				{
					RRLogger.Log($"[{nameof(ServerRouter).Orange()}] Request error : {e}");
				}
				
				RRLogger.Log($"[{nameof(ServerRouter).Orange()}] Retry : {tryCount++}");
			}
			
			return null;
		}
	}
}