#if UNITY_IOS && !UNITY_EDITOR
using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using Unity.Advertisement.IosSupport;
using ATTStatus = Unity.Advertisement.IosSupport.ATTrackingStatusBinding.AuthorizationTrackingStatus;

namespace BerserkV3.Common.AnalyticsSystem
{
	public class ATTrackingPermissionService : IPermissionService
	{
		private CancellationTokenSource internalSource;
		private const int RETRY_FREQUENCY = 5_000;
		private const int REQUEST_TIMEOUT = 180_000;

		public async UniTask RequestPermissionAsync(CancellationToken token)
		{
			try
			{
				internalSource?.Cancel();
				internalSource?.Dispose();
				internalSource = CancellationTokenSource.CreateLinkedTokenSource(token);
				await Task.Delay(1000, token); // wait until main thread unfreeze

				var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
				if (status != ATTStatus.NOT_DETERMINED)
				{
					EndAuthorization();
					return;
				}
				
				RRLogger.Log(GetStatusMessage(status));
				RequestAuthorizationTrackingAsync().Forget();
				await WaitAuthorizationTrackingStatusAsync();
				EndAuthorization();
			}
			catch (Exception e)
			{
				internalSource?.Cancel();
				internalSource?.Dispose();
				internalSource = null;
				RRLogger.Error(e);
			}
		}
		
		private async UniTask RequestAuthorizationTrackingAsync()
		{
			if (internalSource == null)
				return;

			var token = internalSource.Token;
			var start = Environment.TickCount;
			UniTask.WaitUntil(() => Environment.TickCount - start >= REQUEST_TIMEOUT, cancellationToken: token)
				.SuppressCancellationThrow()
				.ContinueWith(_ => EndAuthorization(true))
				.Forget();

			ATTrackingStatusBinding.RequestAuthorizationTracking(); // double start
			var tryCount = 1;
			while (!token.IsCancellationRequested)
			{
				RRLogger.Log($"{nameof(ATTrackingStatusBinding).Orange()} Try RequestAuthorizationTracking : {tryCount++}");
				ATTrackingStatusBinding.RequestAuthorizationTracking(); // once per try
				await UniTask.Delay(RETRY_FREQUENCY, cancellationToken: token).SuppressCancellationThrow();
			}
		}
		
		private async UniTask WaitAuthorizationTrackingStatusAsync()
		{
			if (internalSource == null)
				return;
			
			RRLogger.Log($"{nameof(ATTrackingStatusBinding).Orange()} {nameof(WaitAuthorizationTrackingStatusAsync)}");

			var token = internalSource.Token;
			while (!token.IsCancellationRequested 
			       && ATTrackingStatusBinding.GetAuthorizationTrackingStatus() is ATTStatus.NOT_DETERMINED)
			{
				await UniTask.Yield();
			}
		}

		private void EndAuthorization(bool forceEnd = false)
		{
			if (internalSource == null)
				return;
			
			RRLogger.Error(GetStatusMessage(ATTrackingStatusBinding.GetAuthorizationTrackingStatus(), forceEnd));
			internalSource.Cancel();
			internalSource.Dispose();
			internalSource = null;
		}

		private static string GetStatusMessage(ATTStatus status, bool forceError = false)
		{
			if (forceError || status is ATTStatus.DENIED or ATTStatus.RESTRICTED)
				return $"{nameof(ATTrackingStatusBinding).Red()} {nameof(RequestPermissionAsync)} failed with status {status}";

			return $"{nameof(ATTrackingStatusBinding).Orange()} Current AuthorizationTrackingStatus {status}";
		}
	}
}
#endif