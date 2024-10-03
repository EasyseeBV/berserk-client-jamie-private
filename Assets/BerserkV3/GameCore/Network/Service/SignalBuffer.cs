using System;
using System.Collections.Generic;
using System.Threading;
using BerserkV3.Common.Utils;
using RR.Core.DebugSystem;
using RR.Core.Extensions;

namespace BerserkV3.GameCore.Network
{
	public interface ISignalBuffer<T>
	{
		bool Unpacking { get; }
		int Count { get; }
		event Action<T> OnSignalNotRequired;
		event Action OnExecute;
		event Action OnComplete;
		void Add(T signalItem);
		void Execute(CancellationToken token = default);
		void Clear();
	}

	public abstract class SignalBuffer<T> : DisposableWithCts, ISignalBuffer<T>
	{
		protected readonly Queue<T> BufferItems = new();

		protected bool InternalUnpackingBuffer { get; private set; }

		public bool Unpacking { get; private set; }
		public int Count => BufferItems.Count;
		public event Action<T> OnSignalNotRequired;
		public event Action OnExecute;
		public event Action OnComplete;

		public void Add(T signalItem)
		{
			BufferItems.Enqueue(signalItem);
			OnSignalAdded(signalItem);
		}

		public void Execute(CancellationToken token = default)
		{
			if (InternalUnpackingBuffer || token.IsCancellationRequested)
				return;

			Unpacking = true;
			InternalUnpackingBuffer = true;
			OnExecuting();
			OnExecute?.Invoke();
			var signalsBatch = new List<T>();
			while (BufferItems.Count > 0 && !token.IsCancellationRequested)
			{
				var bufferItem = BufferItems.Dequeue();
				if (!IsValidateSignal(bufferItem))
				{
					RRLogger.Log($"[{"SignalBuffer".Orange().Bold()}] Signal : {bufferItem}," +
					             $"Received but not validated!".Red().Bold());
					OnSignalNotRequired?.Invoke(bufferItem);
					continue;
				}
				
				signalsBatch.Add(bufferItem);
				OnSignalPassed(bufferItem);
			}

			OnSignalsPassed(signalsBatch);

			if (BufferItems.Count > 0 && !token.IsCancellationRequested)
			{
				// repeat if any signals available
				InternalUnpackingBuffer = false;
				Execute(token);
				return;
			}

			Unpacking = false;
			InternalUnpackingBuffer = false;
			if (token.IsCancellationRequested)
				return;

			OnExecuteComplete();
			OnComplete?.Invoke();
		}

		public virtual void Clear()
		{
			BufferItems.Clear();
			Unpacking = false;
			InternalUnpackingBuffer = false;
		}

		public override void Dispose()
		{
			base.Dispose();
			Clear();
			OnSignalNotRequired = null;
			OnExecute = null;
			OnComplete = null;
		}

		protected virtual long CurrentTimestamp()
		{
			return DateTime.UtcNow.Ticks;
		}

		protected virtual bool IsExcludedSignalTimeCheck(T signal)
		{
			return false;
		}

		protected virtual bool IsValidateSignal(T signal)
		{
			return true;
		}

		protected virtual void OnSignalAdded(T signal)
		{
			RRLogger.Log($"[{"SignalBuffer".Orange().Bold()}] {signal} " + "Added to Buffer".Red().Bold());
		}
		
		protected virtual void OnSignalPassed(T signal)
		{
			if (signal == null)
			{
				RRLogger.Log($"[{"SignalBuffer".Orange().Bold()}] Not found signal {"Not Resolved".Red()}");
				return;
			}
			
			RRLogger.Log($"[{"SignalBuffer".Orange().Bold()}] {signal} {"Resolved".Green()}");
		}
		
		protected virtual void OnSignalsPassed(IList<T> signals)
		{
			if (signals.Count <= 0)
			{
				RRLogger.Log($"[{"SignalBuffer".Orange().Bold()}] Not found any signals {"Not Resolved".Red()}");
				return;
			}
			
			RRLogger.Log($"[{"SignalBuffer".Orange().Bold()}] All signals by batch {"Resolved".Green()}");
		}

		protected virtual void OnExecuteComplete()
		{
			RRLogger.Log($"[{"SignalBuffer".Orange().Bold()}] Executing ended.");
		}

		protected virtual void OnExecuting()
		{
			RRLogger.Log($"[{"SignalBuffer".Orange().Bold()}] Executing resolve buffered signals.");
		}
	}
}