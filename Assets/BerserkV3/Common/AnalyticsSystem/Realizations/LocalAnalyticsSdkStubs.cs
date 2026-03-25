using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Local editor-only SDK shims so the client can compile and run without
// proprietary analytics packages. These should never replace real SDKs in builds.
namespace Firebase
{
	public enum DependencyStatus
	{
		Available
	}

	public static class FirebaseApp
	{
		public static Task<DependencyStatus> CheckAndFixDependenciesAsync()
		{
			return Task.FromResult(DependencyStatus.Available);
		}
	}
}

namespace Firebase.Analytics
{
	public sealed class Parameter
	{
		public Parameter(string name, string value)
		{
		}
	}

	public static class FirebaseAnalytics
	{
		public static void SetUserProperty(string name, string value)
		{
		}

		public static void SetAnalyticsCollectionEnabled(bool enabled)
		{
		}

		public static void SetUserId(string userId)
		{
		}

		public static void LogEvent(string eventName, params Parameter[] parameters)
		{
		}
	}
}

namespace GameAnalyticsSDK
{
	public interface IGameAnalyticsATTListener
	{
		void GameAnalyticsATTListenerNotDetermined();
		void GameAnalyticsATTListenerRestricted();
		void GameAnalyticsATTListenerDenied();
		void GameAnalyticsATTListenerAuthorized();
	}

	public static class GameAnalytics
	{
		public static event EventHandler<bool> onInitialize;

		public static void SetCustomId(string userId)
		{
		}

		public static void RequestTrackingAuthorization(IGameAnalyticsATTListener listener)
		{
			listener?.GameAnalyticsATTListenerAuthorized();
		}

		public static void Initialize()
		{
			onInitialize?.Invoke(null, true);
		}

		public static void NewDesignEvent(string eventName, Dictionary<string, object> customData)
		{
		}
	}
}
