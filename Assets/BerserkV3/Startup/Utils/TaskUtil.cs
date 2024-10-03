using System;
using System.Threading;
using System.Threading.Tasks;
using Berserk.Shared.GameCore;
using Berserk.Shared.GameCore.LogicContext;
using RR.Core.Extensions;
using UnityEngine;

namespace BerserkV3.Startup.Utils
{
	public enum TryResult
	{
		Retry,
		Cancel,
		Success,
		Error
	}
	
	public static class TaskUtil
	{
		public static async Task RetryAsync(this Func<Task<TryResult>> func, int? retry = 3, CancellationToken token = default)
		{
			var tryCount = 0;
			var response = TryResult.Retry;
			while (Application.isPlaying && (!retry.HasValue || tryCount < retry))
			{
				try
				{
					response = await func();
					if (token.IsCancellationRequested)
					{
						response = TryResult.Cancel;
						break;
					}
					
					if (response is not TryResult.Retry)
						break;
				}
				catch (Exception e)
				{
					DefaultSharedLogger.Log($"[{nameof(TaskUtil).Orange()}.{nameof(RetryAsync).Orange()}] Method : {func.Method.Name} has exception : {e}");
				}
				
				DefaultSharedLogger.Log($"[{nameof(TaskUtil).Orange()}.{nameof(RetryAsync).Orange()}] Try : {++tryCount}, Method : {func.Method.Name}");
			}
			
			switch (response)
			{
				case TryResult.Success:
					return;
				case TryResult.Cancel:
					throw new OperationCanceledException();
				case TryResult.Error:
					throw new Exception($"Custom application method : {func.Method.Name} - throw an error.");
				
				case TryResult.Retry:
				default:
					throw new Exception($"Retries for : {func.Method.Name} was ended, task uncompleted.");
			}
		}
		
		public static async Task RetryLoopAsync(this Func<Task<TryResult>> func, CancellationToken token = default)
		{
			await RetryAsync(func, null, token);
		}
	}
}