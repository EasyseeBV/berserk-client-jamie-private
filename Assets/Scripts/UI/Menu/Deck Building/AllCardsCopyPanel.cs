using RR.UI.FrameSystem;
using Berserk.Shared.Data.Enums;

namespace UI
{
	public partial class AllCardsCopyPanel : BaseView
	{
		private LoopScrollRefresher loopScrollRefresher;
		public DeckCardCollection CardCollection { get; private set; }
		
		private Faction deckFaction;
		
		public void SetUp(DeckCardCollection cards, Faction deckFaction)
		{
			this.deckFaction = deckFaction;

			loopScrollRefresher = new LoopScrollRefresher(LoopVerticalScrollRect, deckFaction);
			CardCollection = cards;
			CardCollection.OnStackAdded += RefreshPanel;
			CardCollection.OnStackRemoved += RefreshPanel;
			CardCollection.OnCardSortFilter += () => RefreshPanel();

			if (gameObject.activeSelf)
				RefreshPanel();
		}
		
		private void RefreshPanel(IDeckCardStack deckCardStack = null)
		{
			loopScrollRefresher.ScrollToCard(CardCollection.ToFiltered(), deckCardStack);
			SetActive(InfoText, CardCollection.Count == 0);
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			loopScrollRefresher?.Release();
		}
	}
}