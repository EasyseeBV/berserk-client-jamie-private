using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RR.Core
{
    public static class TaskConverters
    {
        public static TaskYieldInstruction ToYieldInstruction(this Task task)
        {
            return new TaskYieldInstruction(task);
        }

        public static TaskYieldInstruction<T> ToYieldInstruction<T>(this Task<T> task)
        {
            return new TaskYieldInstruction<T>(task);
        }

        public static Task WhenAllSequentially(this IEnumerable<Func<Task>> tasks)
        {
            var tcs = new TaskCompletionSource<bool>();

            Task currentTask = Task.FromResult(false);

            foreach (var function in tasks)
            {
                currentTask.ContinueWith(t => tcs.TrySetException(t.Exception.InnerExceptions)
                    , TaskContinuationOptions.OnlyOnFaulted);
                currentTask.ContinueWith(t => tcs.TrySetCanceled()
                    , TaskContinuationOptions.OnlyOnCanceled);
                var continuation = currentTask.ContinueWith(t => function()
                    , TaskContinuationOptions.OnlyOnRanToCompletion);
                currentTask = continuation.Unwrap();
            }

            currentTask.ContinueWith(t => tcs.TrySetException(t.Exception.InnerExceptions)
                , TaskContinuationOptions.OnlyOnFaulted);
            currentTask.ContinueWith(t => tcs.TrySetCanceled()
                , TaskContinuationOptions.OnlyOnCanceled);
            currentTask.ContinueWith(t => tcs.TrySetResult(true)
                , TaskContinuationOptions.OnlyOnRanToCompletion);

            return tcs.Task;
        }

        public static async Task WaitAsync<T>(this TaskCompletionSource<T> tcs, CancellationToken cancellationToken)
        {
            var cts = new CancellationTokenSource();
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken))
            {
                try
                {
                    var exitTok = linkedCts.Token;

                    async Task ListenCancellation()
                    {
                        await Task.Delay(Timeout.Infinite, exitTok).ConfigureAwait(false);
                    }

                    var compoundTask = await Task.WhenAny(
                        tcs.Task,
                        ListenCancellation()
                    ).ConfigureAwait(false);

                    if (compoundTask.IsCanceled)
                        tcs.TrySetCanceled(cancellationToken);
                }
                finally
                {
                    cts.Cancel();
                }
            }
        }
    }

    public class TaskYieldInstruction : YieldInstructionWithResult<bool>
    {
        private Task Task { get; }

        public TaskYieldInstruction(Task task)
        {
            Task = task;
        }

        public override bool keepWaiting
        {
            get
            {
                var ended = Task.IsFaulted || Task.IsCanceled || Task.IsCompleted;
                if (ended)
                    Result = !Task.IsFaulted && !Task.IsCanceled && Task.IsCompleted;

                return !ended;
            }
        }
    }

    public class TaskYieldInstruction<T> : YieldInstructionWithResult<T>
    {
        private Task<T> Task { get; }

        public TaskYieldInstruction(Task<T> task)
        {
            Task = task;
        }

        public override bool keepWaiting
        {
            get
            {
                var ended = Task.IsFaulted || Task.IsCanceled || Task.IsCompleted;
                if (ended)
                {
                    Result = !Task.IsFaulted && !Task.IsCanceled && Task.IsCompleted
                        ? Task.Result
                        : default;
                }

                return !ended;
            }
        }
    }

    public abstract class YieldInstructionWithResult<T> : CustomYieldInstruction
    {
        public T Result { get; protected set; }
    }
}