using NUnit.Framework;
using RR.Core;
using RR.Core.Extensions;
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.TestTools;

namespace Assets.RR.Core.Tests
{
	internal class TaskConvertersTest
    {
        internal event Action NoParametersEvent;
        internal event Action<int> WithParametersEvent;
        internal event DelegateTypeWithoutParameters DelegateTypeEventWithoutParameters;
        internal event DelegateTypeWithParameters DelegateTypeWithParametersEvent;

        private void CallAllEventsAfterDelay()
        {
            CallAllEventsAfterDelay(TimeSpan.FromMilliseconds(100));
        }

        private async void CallAllEventsAfterDelay(TimeSpan delay)
        {
            await Task.Delay(delay);

            NoParametersEvent?.Invoke();
            WithParametersEvent?.Invoke(100);
            DelegateTypeEventWithoutParameters?.Invoke();
            DelegateTypeWithParametersEvent?.Invoke(100);
        }

        [UnityTest]
        public IEnumerator EventToTaskWithNoParameters()
        {
            CallAllEventsAfterDelay();

            var task = EventConverters.FromEventToTask(
                action => NoParametersEvent += action,
                action => NoParametersEvent -= action
            );

            var yieldInstruction = task.ToYieldInstruction();
            yield return yieldInstruction;

            Assert.True(task.IsSucceeded(), "task.IsSucceeded()");
        }

        [UnityTest]
        public IEnumerator EventToTaskWithNoParametersWithCancellationToken()
        {
            var cts = new CancellationTokenSource();

            cts.CancelAfter(TimeSpan.FromMilliseconds(50));
            CallAllEventsAfterDelay(TimeSpan.FromMilliseconds(5000));

            var task = EventConverters.FromEventToTask(
                action => NoParametersEvent += action,
                action => NoParametersEvent -= action,
                cts.Token
            );

            var yieldInstruction = task.ToYieldInstruction();
            yield return yieldInstruction;
            cts.Dispose();

            Assert.True(task.IsCanceled, "task.IsCanceled");
        }

        [UnityTest]
        public IEnumerator EventToTaskWithParameters()
        {
            CallAllEventsAfterDelay();

            var task = EventConverters.FromEventToTask<int>(
                action => WithParametersEvent += action,
                action => WithParametersEvent -= action
            );

            var yieldInstruction = task.ToYieldInstruction();
            yield return yieldInstruction;

            Assert.AreEqual(100, yieldInstruction.Result, "Result is right");
        Assert.True(task.IsSucceeded(), "task.IsSucceeded()");
        }

        [UnityTest]
        public IEnumerator EventToTaskWithParametersWithCancellationToken()
        {
            var cts = new CancellationTokenSource();

            cts.CancelAfter(TimeSpan.FromMilliseconds(50));
            CallAllEventsAfterDelay(TimeSpan.FromMilliseconds(5000));

            var task = EventConverters.FromEventToTask<int>(
                action => WithParametersEvent += action,
                action => WithParametersEvent -= action,
                cts.Token
            );

            var yieldInstruction = task.ToYieldInstruction();
            yield return yieldInstruction;

            Assert.True(yieldInstruction.Result == default, "Result is default");
            Assert.True(task.IsCanceled, "task.IsCanceled");
        }


        [UnityTest]
        public IEnumerator DelegateEventWithoutParametersToTask()
        {
            CallAllEventsAfterDelay();

            var task = EventConverters.FromEventToTask(
                action => new DelegateTypeWithoutParameters(action),
                action => DelegateTypeEventWithoutParameters += action,
                action => DelegateTypeEventWithoutParameters -= action
            );

            var yieldInstruction = task.ToYieldInstruction();
            yield return yieldInstruction;
            
            Assert.True(task.IsSucceeded(), "task.IsSucceeded()");
        }

        [UnityTest]
        public IEnumerator DelegateEventWithoutParametersToTaskWithCancellationToken()
        {
            var cts = new CancellationTokenSource();

            cts.CancelAfter(TimeSpan.FromMilliseconds(50));
            CallAllEventsAfterDelay(TimeSpan.FromMilliseconds(5000));

            var task = EventConverters.FromEventToTask(
                action => new DelegateTypeWithoutParameters(action),
                action => DelegateTypeEventWithoutParameters += action,
                action => DelegateTypeEventWithoutParameters -= action,
                cts.Token
            );

            var yieldInstruction = task.ToYieldInstruction();
            yield return yieldInstruction;

            Assert.True(task.IsCanceled, "task.IsCanceled");
        }

        [UnityTest]
        public IEnumerator DelegateEventWithParametersToTask()
        {
            CallAllEventsAfterDelay();

            var task = EventConverters.FromEventToTask<DelegateTypeWithParameters, int>(
                action => new DelegateTypeWithParameters(action),
                action => DelegateTypeWithParametersEvent += action,
                action => DelegateTypeWithParametersEvent -= action
            );

            var yieldInstruction = task.ToYieldInstruction();
            yield return yieldInstruction;

            Assert.AreEqual(100, yieldInstruction.Result, "Result is right");
            Assert.True(task.IsSucceeded(), "task.IsSucceeded()");
        }

        [UnityTest]
        public IEnumerator DelegateEventWithParametersToTaskWithCancellationToken()
        {
            var cts = new CancellationTokenSource();

            cts.CancelAfter(TimeSpan.FromMilliseconds(50));
            CallAllEventsAfterDelay(TimeSpan.FromMilliseconds(5000));

            var task = EventConverters.FromEventToTask<DelegateTypeWithParameters, int>(
                action => new DelegateTypeWithParameters(action),
                action => DelegateTypeWithParametersEvent += action,
                action => DelegateTypeWithParametersEvent -= action,
                cts.Token
            );

            var yieldInstruction = task.ToYieldInstruction();
            yield return yieldInstruction;

            Assert.True(yieldInstruction.Result == default, "Result is default");
            Assert.True(task.IsCanceled, "task.IsCanceled");
        }
    }

    internal delegate void DelegateTypeWithoutParameters();

    internal delegate void DelegateTypeWithParameters(int parameter);
}