using System;
using BerserkV3.Common.DataBase;
using RR.UI.FrameSystem;
using UnityEngine;
using Vulcan.Data;
using UI;

namespace Statistics
{
	public class FavoriteDeckStatisticView : MultipleStatisticView
	{
		public FavoriteDeckStatisticView(StatisticModel model,
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

			if (view is not CardStatisticView statView)
				throw new InvalidOperationException($"Unknown view type : {view.GetType().Name}, Input Param : {param}");

			int.TryParse(param.Get("Count"), out var count);
			statView.Init(GameDataBaseAdapter.Instance.GetCard(param.Get("Id")), count > 1 ? count.ToString() : string.Empty);
			statView.gameObject.SetActive(true);
			statView.Show(noAnimation:true);
		}
	}
}