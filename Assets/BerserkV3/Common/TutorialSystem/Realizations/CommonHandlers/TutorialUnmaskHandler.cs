using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Data;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;
using RR.Game.TutorialSystemV2.Realizations;
using UnityEngine;
using Zenject;

namespace BerserkV3.Common.TutorialSystem.CommonHandlers
{
    public class TutorialUnmaskHandler : ITutorialInvokeHandler, ITutorialCloseHandler, 
        ITutorialRemoveHandler, ITutorialOrderable
    {
	    private static UITutorialUnmask Window => UITutorialUnmask.Instance;
        private readonly IInstantiator instantiator;
        private readonly ITutorialCameraProvider cameraProvider;
        private readonly ITutorialHandlersRepository handlersRepository;
        private readonly IList<ITutorialHintUnmaskEntity> tickable;
        private CancellationTokenSource tickSource;

        public int Order => 9;

        public TutorialUnmaskHandler(
            IInstantiator instantiator,
            ITutorialHandlersRepository handlersRepository)
        {
            this.instantiator = instantiator;
            this.handlersRepository = handlersRepository;
            tickable = new List<ITutorialHintUnmaskEntity>();
        }

        public Task InvokeAsync(ITutorialHintEntity hint)
        {
            if (hint?.Unmasks == null || hint.Unmasks.Length == 0)
                return Task.CompletedTask;

            tickSource?.Cancel();
            tickSource?.Dispose();
            tickSource = null;
            
            LinkUnmasks(hint).ForEach(unmask =>
            {
	            if (unmask.Data.AutoRefresh && !tickable.Contains(unmask))
		            tickable.Add(unmask);
	            
	            unmask.Refresh();
            });

            if (tickable.Any() && tickSource == null)
            {
	            tickSource = new CancellationTokenSource();
	            RefreshAsync(tickSource.Token).Forget();
            }
            
            Window.Enable(true);
            return Task.CompletedTask;
        }

        public Task CloseAsync(ITutorialHintEntity hint)
        {
            if (hint?.Unmasks == null || hint.Unmasks.Length == 0)
                return Task.CompletedTask;

            tickSource?.Cancel();
            tickSource?.Dispose();
            tickSource = null;
            tickable.Clear();
            Window.Enable(false);
            return Task.CompletedTask;
        }

        public Task RemovedAsync()
        {
	        tickSource?.Cancel();
	        tickSource?.Dispose();
	        tickSource = null;
	        tickable.Clear();
	        Window.Enable(false);
	        return Task.CompletedTask;
        }
        
        private async UniTask RefreshAsync(CancellationToken token)
        {
	        while (Application.isPlaying && tickable.Count > 0)
	        {
		        await UniTask.Yield();
		        token.ThrowIfCancellationRequested();
		        tickable.ForEach(x=> x?.Refresh());
	        }
        }

        private ITutorialHintUnmaskEntity Create(ITutorialUnmaskData data, ITutorialHintTarget target, int index)
        {
	        var unmask = Window.GetUnmask(index);
	        var args = new object[] {unmask, data, target}.Where(x => x != null).ToArray();
            return instantiator.Instantiate<DefaultTutorialHintUnmaskEntity>(args);
        }

        private IEnumerable<ITutorialHintUnmaskEntity> LinkUnmasks(ITutorialHintEntity hint)
        {
            if (hint?.Unmasks == null || hint.Unmasks.Length == 0)
                return Array.Empty<ITutorialHintUnmaskEntity>();
            
            var hintTargets = handlersRepository
                .Get<ITutorialHintTarget>(false, hint.TriggerIds)
                .ToArray();
            
            ITutorialHintTarget TryGetHintTargetByIndex(int index)
            {
                return index >= 0 && index < hintTargets.Length ? hintTargets[index] : default;
            }
            
            return hint.Unmasks.Select((data, index) => Create(data, TryGetHintTargetByIndex(index), index));
        }
    }
}