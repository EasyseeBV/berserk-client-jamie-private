using System.Threading.Tasks;
using BerserkV3.Common.PreviewSystem;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using Statistics;
using UnityEngine.UI;

namespace UI
{
	public partial class ProfileView : BaseView
	{
		private PageView[] pageMap;
		private Button[] tabButtonsMap;
		private Image[] tabSelectionMap;
		private IStatisticApplication statisticApp;
		
		protected override void OnAwake()
		{
			base.OnAwake();
			var statisticRepository = new StatisticRepository();
			var statisticViewFactory = new StatisticViewFactory();
			var statisticEntityFactory = new StatisticViewModelFactory(statisticViewFactory, statisticRepository);
			statisticApp = new StatisticApplication(statisticRepository);
			ProfilePage.Construct(statisticEntityFactory);
			StatisticsPage.Construct(statisticEntityFactory, statisticApp);
			FavoriteDeckPage.Construct(statisticEntityFactory);
			
			pageMap = new PageView[]
			{
				ProfilePage,
				StatisticsPage,
				CustomisationPage,
				FavoriteDeckPage
			};
			
			tabSelectionMap = new []
			{
				TabProfileSelected,
				TabStatisticsSelected,
				TabCustomisationSelected,
				TabFavoriteDeckSelected
			};
			
			tabButtonsMap = new[]
			{
				TabProfileButton,
				TabStatisticsButton,
				TabCustomisationButton,
				TabFavoriteDeckButton
			};
			
			tabButtonsMap.ForEach((button, i) =>
			{
				button.onClick.AddListener(() => Swtich(i));
			});
			
			pageMap.ForEach(page => page.OnSwitchRequired += OnSwitchRequired);
		}
		
		public async UniTask InitAndShowAsync()
		{
			if (!await statisticApp.Init())
				return;

			Swtich(0);
			Show();
		}

		private void Swtich(int pageIndex)
		{
			PreviewSystemAdapter.Instance.Close();
			
			pageMap.ForEach((page, i) =>
			{
				if (pageIndex == i)
				{
					page.InitAndShow();
					Set(TitleText, page.Title);
				}
				else
					page.Close(noAnimation : true);
				
				if(i < tabSelectionMap.Length)
					tabSelectionMap[i].enabled = pageIndex == i;
				
				if(i < tabButtonsMap.Length)
					tabButtonsMap[i].interactable = pageIndex != i;
			});
			var statisticsTabVisible = statisticApp.AvailableStatistic("PlayerOne")
			                       || statisticApp.AvailableStatistic("PlayerTwo");
			
			SetActive(StatisticsTab, statisticsTabVisible);
		}

		private void OnSwitchRequired(string typeName)
		{
			if (string.IsNullOrEmpty(typeName))
			{
				Close();
				return;
			}
			
			Swtich(pageMap.IndexOf(page => page.GetType().Name == typeName));
		}

		protected override void OnClosed()
		{
			PreviewSystemAdapter.Instance.Close();
			
			pageMap.ForEach(page => page.Close(noAnimation: true));
		}
		
		private void OnDestroy()
		{
			statisticApp?.Dispose();
			statisticApp = null;
		}
	}
}