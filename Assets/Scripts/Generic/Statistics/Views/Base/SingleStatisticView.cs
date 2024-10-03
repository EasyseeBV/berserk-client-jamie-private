using System.Linq;
using RR.UI.FrameSystem;
using UnityEngine;

namespace Statistics
{
	public abstract class SingleStatisticView : IStatisticView
	{
		protected BaseView View;
		private readonly StatisticModel model;
		private readonly Transform parentView;
		private readonly IStatisticViewFactory factory;
		
		protected SingleStatisticView(StatisticModel model,
		                              IStatisticViewFactory factory,
		                              Transform parentView = null)
		{
			this.model = model;
			this.parentView = parentView;
			this.factory = factory;
		}



		public void Initialize()
		{
			View ??= factory.Create(model.Id, parentView);
			OnUpdate(View, model.Params.First());
		}

		public void SetActive(bool value)
		{
			if (value)
				View?.Show(noAnimation: true);
			else
				View?.Hide(noAnimation: true);
		}

		public void Dispose()
		{
			OnDispose();
			View?.Close(noAnimation: true);
			View = null;
		}

		protected abstract void OnUpdate(BaseView view, IStatisticParam param);
		
		protected virtual void OnDispose(){}
	}
}