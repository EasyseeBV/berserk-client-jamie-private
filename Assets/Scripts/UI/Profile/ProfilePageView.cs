using System.Collections.Generic;
using RR.Core.Extensions;
using Statistics;
using UnityEngine;

namespace UI
{
	public partial class ProfilePageView : PageView
	{
		public override string Title => "Profile";
		
		private IStatisticViewModelFactory statisticsFactory;
		private Dictionary<string, IStatisticView> statisticViews;
		
		public void Construct(IStatisticViewModelFactory factory)
		{
			statisticsFactory = factory;
			statisticViews = new Dictionary<string, IStatisticView>();
			SeeMoreButton.onClick.AddListener(() => SentRequestSwitchPage(nameof(FavoriteDeckPageView)));
			CloseButton.Subscribe(() => SentRequestSwitchPage());
		}

		public override void InitAndShow()
		{
			Clear();
			InitStatistic("FavoriteDeck", FavoriteDeckLayout, 5);
			InitStatistic("FavoriteFaction", FavoritsLayout);
			InitStatistic("FavoriteVulcanite", FavoritsLayout);
			InitStatistic("InGameStats", InGameLayout);
			InitStatistic("MMR", UserStatsLayout);
			
			SetActive(SeeMoreButton, FavoriteDeckLayout.childCount > 5);
			SetActive(FavoriteDeckText, statisticViews.ContainsKey("FavoriteDeck"));
			Show(noAnimation: true);
		}

		private void InitStatistic(string id, Transform parent = null, int displayCount = 0)
		{
			var statisticView = statisticsFactory.Create(id, parent, displayCount);
			statisticView?.Initialize();
			if(statisticView != null)
				statisticViews.Add(id, statisticView);
		}

		private void Clear()
		{
			statisticViews?.Values.ForEach(statisticView => statisticView?.Dispose());
			statisticViews?.Clear();
		}

		protected override void OnHidden()
		{
			base.OnHidden();
			Clear();
		}

		protected override void OnClosed()
		{
			base.OnClosed();
			Clear();
		}
	}
}