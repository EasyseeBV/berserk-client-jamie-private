using System;
using RR.UI.FrameSystem;
using UI;
using UnityEngine;

namespace Statistics
{
	public class InGameStatisticView : MultipleStatisticView
	{
		public InGameStatisticView(StatisticModel model,
		                           IStatisticViewFactory viewFactory,
		                           Transform parent = null,
		                           int displayCount = 0)
			: base(model, viewFactory, parent, displayCount)
		{
		}

		protected override void OnInitialize(BaseView view, IStatisticParam param)
		{
			if (view == null)
				throw new NullReferenceException($"View is missing, Input Param : {param}");

			if (!(view is BaseStatisticView statView))
				throw new InvalidOperationException($"Unkown view type : {view.GetType().Name}, Input Param : {param}");

			statView.SetActiveImage(false);
			statView.SetActiveImageMask(false);

			statView.SetTextKey(param.Get("Title"));
			statView.SetActiveTextKey(true);
			statView.SetActiveTextValue(true);
			statView.SetTextValue(param.Get("Value"));

			statView.SetActiveBackground(false);
			statView.SetActiveTextBackground(false);
			statView.SetActive(true);
		}
	}
}