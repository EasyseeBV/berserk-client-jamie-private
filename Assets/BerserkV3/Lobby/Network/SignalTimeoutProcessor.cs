using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.GameCore.LogicContext;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Lobby.Network
{
	public class SignalTimeoutProcessor : ISignalTimeoutProcessor, IDisposable
	{
		private readonly IGameDatabase gameDatabase;
		private readonly Dictionary<string, UniTaskCompletionSource> waitResponces = new();
		private readonly List<(string Key, DateTime ResponseTime)> responseBuffer = new();
		private readonly object bufferLock = new();
		private readonly object waitLock = new();
		
		public int TimeoutMs { get; set; } = 10_000;
		public int TimeoutResponseBufferMs { get; set; } = 5_000;
		public bool Enabled { get; set; }

		public SignalTimeoutProcessor(IGameDatabase gameDatabase)
		{
			this.gameDatabase = gameDatabase;
		}

		public void ResponseReceived(object target)
		{
			if (!Enabled)
				return;
			
			var key = target.ToString();
			
			lock (bufferLock)
			{
				responseBuffer.Add((key, DateTime.UtcNow));
			}

			ResponseBufferTimeoutAsync(key).Forget();

			lock (waitLock)
			{
				if (!waitResponces.TryGetValue(key, out var cts))
					return;

				cts?.TrySetResult();
				waitResponces.Remove(key);
			}
		}

		public async UniTask<bool> WaitResponseAsync(object target, bool useResponseBuffer = false)
		{
			try
			{
				if (!Enabled)
					return true;
				
				var key = target.ToString();
				if (useResponseBuffer && HasInBuffer(key))
					return true;

				UniTaskCompletionSource tcs;

				lock (waitLock)
				{
					if (!waitResponces.TryGetValue(key, out tcs))
					{
						tcs = new UniTaskCompletionSource();
						ResponseTimeoutAsync(tcs).Forget();
					}
					
					waitResponces.TryAdd(key, tcs);
				}

				if (!useResponseBuffer || !HasInBuffer(key))
					await tcs.Task.Preserve();
				
				return true;
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
				return false;
			}
		}

		public void Clear()
		{
			lock (waitLock)
			{
				foreach (var tcs in waitResponces.Values.ToArray())
					tcs?.TrySetResult();
				
				waitResponces.Clear();
			}

			lock (bufferLock)
			{
				responseBuffer.Clear();
			}
		}

		private bool HasInBuffer(string key)
		{
			if (!Enabled)
				return false;
			
			lock (bufferLock)
			{
				return responseBuffer.Any(x => x.Key == key);
			}
		}

		private async UniTask ResponseTimeoutAsync(UniTaskCompletionSource tcs)
		{
			if (!Enabled)
				return;

			var timeOut = DateTime.UtcNow.Add(TimeSpan.FromMilliseconds(TimeoutMs));
			await UniTask.WaitWhile(() => !tcs.Task.Status.IsCompleted() && timeOut > DateTime.UtcNow && Enabled);

			if (!tcs.Task.Status.IsCompleted() && Enabled)
			{
				tcs.TrySetException(new TimeoutException(gameDatabase.GetLocalization("ClientSignalR_ResponseTimeout_Exception")));
				lock (waitLock)
				{
					var responce = waitResponces.FirstOrDefault(x => x.Value == tcs);

					if (!string.IsNullOrEmpty(responce.Key))
						waitResponces.Remove(responce.Key);
				}
			}
		}

		private async UniTask ResponseBufferTimeoutAsync(string key)
		{
			if (!Enabled)
				return;
			
			lock (bufferLock)
			{
				if (!responseBuffer.Any())
					return;
			}

			var delay = TimeSpan.FromMilliseconds(TimeoutResponseBufferMs);
			await UniTask.Delay(delay);
			var timeout = DateTime.UtcNow - delay;
			lock (bufferLock)
			{
				responseBuffer.RemoveAll(x => x.Key == key && x.ResponseTime <= timeout);
			}
		}

		public void Dispose()
		{
			Clear();
		}
	}
}