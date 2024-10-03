using System;
using System.Collections.Generic;
using System.Threading;
using RR.Core.Extensions;
using UnityEngine;

namespace RR.Core.Async
{
    public class Dispatcher : MonoBehaviour
    {
	    public static Dispatcher Instance { get; protected set; }


		private readonly Queue<Action> taskQueue = new Queue<Action>();

        private Thread thread;

        protected void Awake()
        {
            thread = Thread.CurrentThread;
        }

        private void Update()
        {
            if (Application.isPlaying && Thread.CurrentThread == thread)
                DispatchTasks();
        }

        public void Dispatch(Action action) => Queue(action);
        public void Queue(Action action)
        {
            if (Thread.CurrentThread == thread)
            {
                action();
                return;
            }

            lock (taskQueue)
            {
                taskQueue.Enqueue(action);
            }
        }

        private void DispatchTasks()
        {
            lock (taskQueue)
            {
                if (taskQueue.Count == 0)
                    return;

                try
                {
                    taskQueue.Dequeue()?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
            }
        }

        public static Dispatcher Get()
        {
	        return Instance.Value() ?? new GameObject().GetOrAddComponent<Dispatcher>();
        }
    }
}