using System;

namespace RR.Core
{
    // TODO: Inspect/Remove
    public sealed class Disposable : IDisposable
    {
        private IDisposable DisposableObject { get; }
        private readonly bool allowLateInitOfAction;
        private Action doOnDispose;

        public Action DoOnDispose
        {
            get => doOnDispose;
            set => SetDoOnDispose(value);
        }

        public Disposable()
        {
            allowLateInitOfAction = true;
        }

        public Disposable(Action doOnDispose) : this(null, doOnDispose)
        {
        }

        public Disposable(IDisposable disposable, Action doOnDispose = null)
        {
            DisposableObject = disposable;
            this.doOnDispose = doOnDispose;
        }

        public void Dispose()
        {
            DoOnDispose?.Invoke();
            DisposableObject?.Dispose();
        }

        private void SetDoOnDispose(Action action)
        {
            if (allowLateInitOfAction)
                doOnDispose = action;
        }
    }
}
