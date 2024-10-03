using System.Collections.Generic;
using RR.Core.DebugSystem;

namespace BerserkV3.Common.AnalyticsSystem
{
	public interface IAnalyticsModel
	{
		string Key { get; }

		Dictionary<string, object> ToDictionary();
	}

	public abstract class AnalyticsModel : IAnalyticsModel
	{
		protected IAnalyticsModel AnalyticsModelCore;
		private Dictionary<string, object> modelParams = new();

		public string Key { get; protected set; }

		public Dictionary<string, object> ToDictionary()
		{
			FillData();
			return modelParams;
		}

		protected bool AddModelParameter(string key, object value)
		{
			if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
			{
				RRLogger.Warning($"Cannot add empty value for {key}");
				return false;
			}

			if (modelParams.ContainsKey(key))
			{
				RRLogger.Error($"Event parameter with key{key} has already been added!");
				return false;
			}

			if (modelParams.Count >= 10)
			{
				// This is unity analytics limitation
				RRLogger.Error("Cannot add more than 10 event parameters!");
				return false;
			}

			// TODO: Add 500 bytes limit check
			// https://docs.unity3d.com/Manual/UnityAnalyticsEventLimits.html

			modelParams.Add(key, value);
			return true;
		}

		protected virtual void FillData()
		{
			AddModelCore();
		}

		/// <summary>
		///     Adds general parameters to the model
		/// </summary>
		/// <returns> AnalyticsModel </returns>
		private void AddModelCore()
		{
			AnalyticsModelCore = new AnalyticsModelCore();
			modelParams = AnalyticsModelCore.ToDictionary();
		}
	}
}