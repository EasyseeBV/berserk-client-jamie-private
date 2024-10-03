using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RR.Core.Extensions;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;
using RR.Game.TutorialSystemV2.Data;
using RR.Game.TutorialSystemV2.Realizations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RR.Game.TutorialSystemV2.Presentation
{
    public abstract class BaseTutorialHintTarget : MonoBehaviour, 
	    ITutorialHintTarget, ITutorialInvokeHandler, ITutorialCloseHandler, 
	    ITutorialRemoveHandler, ITutorialOrderable, ITutorialIdentity
    {
	    protected class ExternalSubscriber : IDisposable
        {
            private readonly List<UnityEvent> triggers = new();
            private readonly List<UnityAction> callbacks = new();
            private UnityAction unscribeCallBack;
            private bool disposed;
            
            public ExternalSubscriber Subscribe(UnityEvent trigger, UnityAction action)
            {
                if (disposed || trigger == null || action == null)
                    return this;
                
                triggers.Add(trigger);
                callbacks.Add(action);
                trigger.AddListener(action);
                return this;
            }
            public ExternalSubscriber UnSubscribe(UnityAction action)
            {
                if (action == null)
                    return this;
                unscribeCallBack = action;
                return this;
            }

            public void Dispose()
            {
                if (disposed)
                    return;
                
                disposed = true;
                triggers.ForEach((trigger, index) =>
                {
                    trigger?.RemoveListener(callbacks[index]);
                });
                unscribeCallBack?.Invoke();
                unscribeCallBack = null;
                callbacks.Clear();
                triggers.Clear();
            }
        }
        
        [Tooltip("Execution order, by default 0")]
        [SerializeField] protected int order;
        [Tooltip("Destroy the target if tutorial is already completed when it is initialized?")]
        [SerializeField] protected bool destroyIfCompleted = true;
        [Tooltip("Control the target size.")]
        [SerializeField] private TutorialVector3 sizeScale = Vector3.one;
        [Tooltip("Unique Ids of the tutorial hint, as in the config, if it empty, will be used Object.name")]
        [SerializeField] protected string[] triggerIds = Array.Empty<string>();
        [Tooltip("Buttons to complete the current step.")]
        [SerializeField] protected Button[] closeButtons = Array.Empty<Button>();
        [Space]
        [Tooltip("Manage your Objects when hint will be invoked.")]
        [SerializeField] protected UnityEvent onInvoke = new();
        [Tooltip("Manage your Objects when hint will be closed.")]
        [SerializeField] protected UnityEvent onClose = new();
        
        protected bool Initialized { get; private set; }

        public Transform Transform => transform;
        public abstract TutorialVector3 Position { get; }
        public virtual TutorialVector3 Size => SizeScale;
        public virtual TutorialVector3 SizeScale { get => sizeScale; set => sizeScale = value; }
        public virtual string[] Ids => triggerIds == null || triggerIds.Length == 0 ? new[]{name} : triggerIds;
        public virtual int Order => order;
        
        public BaseTutorialHintTarget Init()
        {
	        if (Initialized)
		        return this;
	        
	        Initialized = true;
	        if (TutorialAdapter.ProgressRepository.IsCompleted())
	        {
		        if (destroyIfCompleted) 
			        Destroy(this);
                
		        return this;
	        }
	        
	        TutorialAdapter.HandlersRepository.Register(this);
	        OnInit();
	        return this;
        }
        
        public void Dispose()
        {
	        if (!Initialized)
		        return;
            
	        Initialized = false;
	        onInvoke?.RemoveAllListeners();
	        onClose?.RemoveAllListeners();
	        closeButtons?.ForEach(validButton =>
	        {
		        if (!validButton)
			        return;
                
		        validButton.onClick.RemoveListener(HandleClick);
	        });
	        TutorialAdapter.HandlersRepository.UnRegister(this);
	        OnDisposed();
        }
        
        private void Start()
        {
	        Init();
        }
        
        /// <summary>
        /// Setup dynamic trigger ids for hint target
        /// </summary>
        /// <param name="ids">trigger ids, keywords, by default UnityEngine.Object.name</param>
        public virtual void SetTriggerIds(params string[] ids)
        {
            triggerIds = ids;
        }
        
        /// <summary>
        /// Subscribe any external dynamic buttons
        /// </summary>
        /// <param name="buttons"></param>
        /// <returns></returns>
        public virtual IDisposable Subscribe(params Button[] buttons)
        {
            var subscriber = new ExternalSubscriber();
            buttons.Where(validButton => validButton).ForEach(validButton =>
            {
                if (!validButton)
                    return;
                
                subscriber
                    .Subscribe(onInvoke, () => validButton.onClick.AddListener(HandleClick))
                    .Subscribe(onClose, () => validButton.onClick.RemoveListener(HandleClick))
                    .UnSubscribe(() => validButton.onClick.RemoveListener(HandleClick));
            });
            
            return subscriber;
        }

        public virtual Task InvokeAsync(ITutorialHintEntity hint)
        {
            closeButtons?.ForEach(x=> x.onClick.AddListener(HandleClick));
            onInvoke?.Invoke();
            return Task.CompletedTask;
        }
        
        public virtual Task CloseAsync(ITutorialHintEntity hint)
        {
            closeButtons?.ForEach(x=> x.onClick.RemoveListener(HandleClick));
            onClose?.Invoke();
            return Task.CompletedTask;
        }
        
        public virtual Task RemovedAsync()
        {
            Dispose();
            return Task.CompletedTask;
        }

        public override string ToString()
        {
            return $"[{GetType().Name.Orange()}.{name.Orange()}] Ids : {string.Join(", ", Ids)}";
        }
        
        protected virtual void OnInit(){}

        protected virtual void OnDisposed(){}
        
        protected virtual void OnDestroy()
        {
	        Dispose();
        }

        protected virtual void HandleClick()
        {
	        HandleClickAsync().Forget();
        }
        
        protected virtual async Task HandleClickAsync()
        {
            if (!Initialized)
                return;
            
            await TutorialAdapter.Application.CloseAsync();
        }
    }
}