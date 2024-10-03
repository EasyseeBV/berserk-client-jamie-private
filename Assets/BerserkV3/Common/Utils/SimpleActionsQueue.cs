using System;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

namespace BerserkV3.Common.Utils
{
	public interface IActionsQueue
	{
		event Action<string, string, int> OnActionEnqueued;

		public UniTask EnqueueAsync(Func<UniTask> func, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0);

		public UniTask EnqueueAsync(Action func, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0);

		void Enqueue(Func<UniTask> func, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0);

		void Enqueue(Action func, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0);
	}

	public class SimpleActionsQueue : DisposableWithCts, IActionsQueue
	{
		public event Action<string, string, int> OnActionEnqueued;

		private readonly Channel<Func<UniTask>> actionsChannel;

		public SimpleActionsQueue()
		{
			actionsChannel = Channel.CreateSingleConsumerUnbounded<Func<UniTask>>();

			actionsChannel.Reader.ReadAllAsync(Token)
				.SubscribeAwait(OnActionAsyncHandler, Token);
		}

		private static UniTask OnActionAsyncHandler(Func<UniTask> action)
		{
			return action.Invoke();
		}

		public UniTask EnqueueAsync(Func<UniTask> func, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0)
		{
			var tcs = new UniTaskCompletionSource();

			actionsChannel.Writer.TryWrite(() => func.Invoke().ContinueWith(tcs.TrySetResult));

			OnActionEnqueued?.Invoke(filePath, memberName, lineNumber);

			return tcs.Task;
		}

		public void Enqueue(Func<UniTask> func, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0)
		{
			actionsChannel.Writer.TryWrite(func);

			OnActionEnqueued?.Invoke(filePath, memberName, lineNumber);
		}

		public void Enqueue(Action func, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0)
		{
			actionsChannel.Writer.TryWrite(() =>
			{
				func.Invoke();

				return UniTask.CompletedTask;
			});

			OnActionEnqueued?.Invoke(filePath, memberName, lineNumber);
		}

		public UniTask EnqueueAsync(Action func, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0)
		{
			var tcs = new UniTaskCompletionSource();

			actionsChannel.Writer.TryWrite(() =>
			{
				func.Invoke();
				tcs.TrySetResult();

				return UniTask.CompletedTask;
			});

			OnActionEnqueued?.Invoke(filePath, memberName, lineNumber);

			return tcs.Task;
		}

		public override void Dispose()
		{
			base.Dispose();

			actionsChannel.Writer.TryComplete();
		}
	}
}