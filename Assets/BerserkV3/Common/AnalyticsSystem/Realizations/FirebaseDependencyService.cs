using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Firebase;
using RR.Core.DebugSystem;

namespace BerserkV3.Common.AnalyticsSystem
{
	public class FirebaseDependencyService : IPermissionService
	{
		private static readonly int TimeOut = 10;
		public async UniTask RequestPermissionAsync(CancellationToken token)
		{
			try
			{
				var dependencyStatus = await FirebaseApp
						.CheckAndFixDependenciesAsync()
						.AsUniTask()
						.Timeout(TimeSpan.FromSeconds(TimeOut));

				if (dependencyStatus != DependencyStatus.Available)
					throw new Exception($"Could not resolve all Firebase dependencies: {dependencyStatus}");

				RRLogger.Log($"{nameof(FirebaseApp)} success with status {dependencyStatus}");
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}
	}
}