using System;
using RR.UI.FrameSystem;
using UnityEngine;
using UI;

namespace Statistics
{
	public class StatisticViewFactory : IStatisticViewFactory
	{
		public BaseView Create(string id, Transform parent = null)
		{
			var view = GetSourceView(id).Clone();
			view.transform.SetParent(parent, false);
			view.SetDynamicallyCreated(true);
			return view;
		}

		private BaseView GetSourceView(string id)
		{
			return id switch
			{
				"MMR" => HorizontalStatisticView.Instance,
				"FavoriteDeck" => CardStatisticView.Instance,
				"FavoriteFaction" => VulcaniteStatisticView.Instance,
				"FavoriteVulcanite" => VulcaniteStatisticView.Instance,
				"InGameStats" => HorizontalStatisticView.Instance,
				"PlayerOne" => VulcaniteStatisticView.Instance,
				"PlayerTwo" => VulcaniteStatisticView.Instance,
				_ => throw new NotImplementedException($"View with id : '{id}' does not impemented in factory")
			};
		}
	}
}