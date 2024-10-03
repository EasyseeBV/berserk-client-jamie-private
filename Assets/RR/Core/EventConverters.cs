using System;
using System.Threading;
using System.Threading.Tasks;

namespace RR.Core
{
    public static class EventConverters
    {
        public static IDisposable FromEvent<TDelegate, TArg>(
            Func<Action<TArg>, TDelegate> conversion,
            Action<TDelegate> addHandler,
            Action<TDelegate> removeHandler,
            Func<IDisposable, Action<TArg>> handler
        ) where TDelegate : Delegate
        {
            var disposable = new Disposable();
            var callableHandler = handler.Invoke(disposable);

            var @delegate = conversion(eventArgs => { callableHandler(eventArgs); });

            disposable.DoOnDispose = () => { removeHandler(@delegate); };
            addHandler(@delegate);

            return disposable;
        }

        public static IDisposable FromEvent<TDelegate>(
            Func<Action, TDelegate> conversion,
            Action<TDelegate> addHandler,
            Action<TDelegate> removeHandler,
            Func<IDisposable, Action> handler
        ) where TDelegate : Delegate
        {
            var disposable = new Disposable();
            var callableHandler = handler.Invoke(disposable);

            var @delegate = conversion(() => { callableHandler(); });

            disposable.DoOnDispose = () => { removeHandler(@delegate); };
            addHandler(@delegate);

            return disposable;
        }

        public static IDisposable FromEvent<TArg>(
            Action<Action<TArg>> addHandler,
            Action<Action<TArg>> removeHandler,
            Func<IDisposable, Action<TArg>> handler
        )
        {
            var disposable = new Disposable();
            var callableHandler = handler.Invoke(disposable);

            disposable.DoOnDispose = () => { removeHandler(callableHandler); };
            addHandler(callableHandler);

            return disposable;
        }

        public static IDisposable FromEvent(
            Action<Action> addHandler,
            Action<Action> removeHandler,
            Func<IDisposable, Action> handler
        )
        {
            var disposable = new Disposable();
            var callableHandler = handler.Invoke(disposable);

            disposable.DoOnDispose = () => { removeHandler(callableHandler); };
            addHandler(callableHandler);

            return disposable;
        }

        // ==================

        public static Task FromEventToTask(
            Action<Action> addHandler,
            Action<Action> removeHandler,
            CancellationToken? cancellationToken = null
        )
        {
            var tcs = new TaskCompletionSource<bool>();

            var disposable = FromEvent(addHandler, removeHandler, eventDisposable => () =>
            {
                try
                {
                    tcs.TrySetResult(true);
                }
                finally
                {
                    eventDisposable.Dispose();
                }
            });

            new Action(async () =>
            {
                await tcs.WaitAsync(cancellationToken ?? CancellationToken.None);
                disposable?.Dispose();
            })();

            return tcs.Task;
        }

        public static Task<TArg> FromEventToTask<TArg>(
            Action<Action<TArg>> addHandler,
            Action<Action<TArg>> removeHandler,
            CancellationToken? cancellationToken = null
        )
        {
            var tcs = new TaskCompletionSource<TArg>();

            var disposable = FromEvent(addHandler, removeHandler, eventDisposable => arg =>
            {
                try
                {
                    tcs.TrySetResult(arg);
                }
                finally
                {
                    eventDisposable.Dispose();
                }
            });

            new Action(async () =>
            {
                await tcs.WaitAsync(cancellationToken ?? CancellationToken.None);
                disposable?.Dispose();
            })();

            return tcs.Task;
        }

        public static Task<TArg> FromEventToTask<TDelegate, TArg>(
            Func<Action<TArg>, TDelegate> conversion,
            Action<TDelegate> addHandler,
            Action<TDelegate> removeHandler,
            CancellationToken? cancellationToken = null
        ) where TDelegate : Delegate
        {
            var tcs = new TaskCompletionSource<TArg>();

            var disposable = FromEvent(conversion, addHandler, removeHandler, eventDisposable => arg =>
            {
                try
                {
                    tcs.TrySetResult(arg);
                }
                finally
                {
                    eventDisposable.Dispose();
                }
            });

            new Action(async () =>
            {
                await tcs.WaitAsync(cancellationToken ?? CancellationToken.None);
                disposable?.Dispose();
            })();

            return tcs.Task;
        }

        public static Task FromEventToTask<TDelegate>(
            Func<Action, TDelegate> conversion,
            Action<TDelegate> addHandler,
            Action<TDelegate> removeHandler,
            CancellationToken? cancellationToken = null
        ) where TDelegate : Delegate
        {
            var tcs = new TaskCompletionSource<bool>();

            var disposable = FromEvent(conversion, addHandler, removeHandler, eventDisposable => () =>
            {
                try
                {
                    tcs.TrySetResult(true);
                }
                finally
                {
                    eventDisposable.Dispose();
                }
            });

            new Action(async () =>
            {
                await tcs.WaitAsync(cancellationToken ?? CancellationToken.None);
                disposable?.Dispose();
            })();

            return tcs.Task;
        }
    }
}