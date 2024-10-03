using Statistics;

namespace UI
{
	public partial class StatisticsPageView : PageView
	{
		public override string Title => "Statistics";
		
		private readonly string playerOneId = "PlayerOne";
		private readonly string playerTwoId = "PlayerTwo";
		private IStatisticViewModelFactory statisticFactory;
		private IStatisticApplication statisticApplication;
		private IStatisticView statisticView;
		private string currentId;
		
		public void Construct(IStatisticViewModelFactory factory, IStatisticApplication application)
		{
			statisticFactory = factory;
			statisticApplication = application;
			CloseButton.Subscribe(() => SentRequestSwitchPage());
			Subscribe(Player1Button, () => InitStatistic(playerOneId));
			Subscribe(Player2Button, () => InitStatistic(playerTwoId));
		}

		public override void InitAndShow()
		{
			var playerOneAvailable = statisticApplication.AvailableStatistic(playerOneId);
			SetActive(Player1Button, playerOneAvailable);
			SetActive(Player2Button, statisticApplication.AvailableStatistic(playerTwoId));
			InitStatistic(playerOneAvailable 
				              ? playerOneId
				              : playerTwoId);
			Show(noAnimation: true);
		}

		private void InitStatistic(string id)
		{
			Clear();
			statisticView = statisticFactory.Create(id, Content.rectTransform);
			statisticView?.Initialize();
			SetActive(Player1OverlayImage, id == playerOneId);
			SetActive(Player2OverlayImage, id == playerTwoId);
			SetInteractable(Player1Button, id == playerTwoId);
			SetInteractable(Player2Button, id == playerOneId);
		}
		
		private void Clear()
		{
			statisticView?.Dispose();
			statisticView = null;
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