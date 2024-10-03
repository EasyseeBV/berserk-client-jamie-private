using System;
using System.Threading.Tasks;
using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.Startup.Network;
using BerserkV3.Startup.Utils;
using RR.Core.DebugSystem;

namespace BerserkV3.Common.AppTime
{

	public class AppTimeSynchronizer : SharedTime, IAppTimeSynchronizer, IDisposable
	{
		private DateTime remote;
		private DateTime appSync;
		private bool isDisposed;
				
		public override DateTime Current => remote + (DateTime.UtcNow - appSync); // the remote time plus local passed time from the last sync.

		public AppTimeSynchronizer()
		{
			appSync = remote = DateTime.UtcNow;
		}
		
		public async Task<TryResult> SynchronizeTimeAsync()
		{
			if (isDisposed)
				return TryResult.Error;
			
			try
			{
				var startSync = DateTime.UtcNow;
				var result = await IdentityAPI.GetApiTime();
				if (!result || result.Data == null)
					return TryResult.Retry;
				
				remote = result.Data.Date;
				appSync = DateTime.UtcNow - (DateTime.UtcNow - startSync) / 2f; // synchronized time minus average synchronization time 
				
				return TryResult.Success;
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
				return TryResult.Retry;
			}
		}
		
		public void Dispose()
		{
			isDisposed = true;
		}
	}

}