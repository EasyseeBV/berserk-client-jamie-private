using Statistics;

namespace UI
{
	public partial class FavoriteDeckPageView : PageView
	{
		public override string Title => "Favorite Deck";

		private IStatisticViewModelFactory statisticsFactory;
		private IStatisticView statisticView;

		public void Construct(IStatisticViewModelFactory factory)
		{
			statisticsFactory = factory;
			CloseButton.Subscribe(() => SentRequestSwitchPage());
		}

		public override void InitAndShow()
		{
			Clear();
			statisticView = statisticsFactory.Create("FavoriteDeck", FavoriteDeckLayout.rectTransform);
			statisticView?.Initialize();
			Show(noAnimation: true);
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