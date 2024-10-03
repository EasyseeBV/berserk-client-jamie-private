using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RR.Core.Extensions;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Data;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;
using RR.Game.TutorialSystemV2.Realizations;
using RR.UI.FrameSystem;
using UnityEngine;

namespace RR.Game.TutorialSystemV2.Presentation
{
	public abstract class BaseTutorialUnmask : BaseView, 
		ITutorialInvokeHandler, ITutorialCloseHandler, ITutorialRemoveHandler, ITutorialOrderable
	{
		protected readonly IList<ITutorialHintUnmaskEntity> Unmasks = new List<ITutorialHintUnmaskEntity>();
		public virtual int Order => 9;

		protected override void Start()
		{
			base.Start();
			TutorialAdapter.HandlersRepository.Register(this);
			Init();
		}

		public virtual async Task InvokeAsync(ITutorialHintEntity hint)
		{
			if (hint?.Unmasks == null || hint.Unmasks.Length == 0)
				return;
            
			CreateEntities(hint).ForEach(entity =>
			{
				Unmasks.Add(entity);
				entity.Refresh();
			});
			
			await ShowViewAsync();
		}

		public virtual async Task CloseAsync(ITutorialHintEntity hint)
		{
			if (hint?.Unmasks == null || hint.Unmasks.Length == 0)
				return;

			await CloseViewAsync();
			ResetToDefault();
		}

		public virtual async Task RemovedAsync()
		{
			await CloseViewAsync();
			ResetToDefault();
			UnInit();
		}

		protected abstract void Init();

		protected abstract void UnInit();

		protected abstract Task ShowViewAsync();

		protected abstract Task CloseViewAsync();

		protected abstract RectTransform CreateUnmask(int index);

		/// <summary>
		/// Create entity to handle unmask data and link with target
		/// </summary>
		/// <param name="data">Can't be null</param>
		/// <param name="target">Can be null</param>
		/// <param name="index">Creation index</param>
		/// <returns></returns>
		protected virtual ITutorialHintUnmaskEntity CreateEntity(
			ITutorialUnmaskData data, 
			ITutorialHintTarget target,
			int index)
		{
			var unmask = CreateUnmask(index);
			unmask.SetSiblingIndex(index);
			return new DefaultTutorialHintUnmaskEntity(CreateUnmask(index), data, TutorialAdapter.CameraProvider, target);
		}
		
		protected virtual IEnumerable<ITutorialHintUnmaskEntity> CreateEntities(ITutorialHintEntity hint)
		{
			if (hint?.Unmasks == null || hint.Unmasks.Length == 0)
				return Array.Empty<ITutorialHintUnmaskEntity>();
            
			var hintTargets = TutorialAdapter.HandlersRepository
				.Get<ITutorialHintTarget>(false, hint.TriggerIds)
				.ToArray();
            
			ITutorialHintTarget TryGetHintTargetByIndex(int index)
			{
				try
				{
					return hintTargets[index];
				}
				catch // that is normal flow, we are expectation an exception.
				{
					return default;
				}
			}

			return hint.Unmasks.Select((unmask, index) => CreateEntity(unmask, TryGetHintTargetByIndex(index), index));
		}

		protected virtual void ResetToDefault()
		{
			Unmasks.Clear();
		}

		protected virtual void LateUpdate()
		{
			if (Unmasks.Count > 0)
				Unmasks.Where(x=> x.Data.AutoRefresh).ForEach(x=> x.Refresh());
		}
		
		protected virtual void OnDestroy()
		{
			TutorialAdapter.HandlersRepository.UnRegister(this);
		}
	}
}