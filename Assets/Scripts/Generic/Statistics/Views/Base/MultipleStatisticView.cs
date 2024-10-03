using System.Collections.Generic;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using UnityEngine;

namespace Statistics
{
	public abstract class MultipleStatisticView : IStatisticView
	{
		private readonly int displayCount;
		private readonly StatisticModel model;
		private readonly Transform parentView;
		protected readonly List<BaseView> Views;
		private readonly IStatisticViewFactory viewFactory;
		private readonly IStatisticsRepository repository;

		protected MultipleStatisticView(StatisticModel model,
		                                IStatisticViewFactory viewFactory,
		                                Transform parentView = null,
		                                int displayCount = 0)
		{
			this.model = model;
			this.parentView = parentView;
			this.viewFactory = viewFactory;
			this.displayCount = displayCount;
			Views = new List<BaseView>();
		}

		public void Initialize()
		{
			Clear();
			
			var targetCount = displayCount > 0
				? Mathf.Min(displayCount, model.Params.Count)
				: model.Params.Count;

			while (targetCount != Views.Count)
			{
				if (Views.Count < targetCount)
				{
					Views.Add(viewFactory.Create(model.Id, parentView));
					continue;
				}

				Views[0].Close(noAnimation: true);
				Views.RemoveAt(0);
			}

			Views.ForEach((view, i) => OnInitialize(view, model.Params[i]));
		}
		
		public void Dispose()
		{
			OnDispose();
			Clear();
		}

		protected void Clear()
		{
			Views.ForEach(view => view?.Close(noAnimation: true));
			Views.Clear();
		}

		protected abstract void OnInitialize(BaseView view, IStatisticParam param);

		protected virtual void OnDispose()
		{
		}
	}
}