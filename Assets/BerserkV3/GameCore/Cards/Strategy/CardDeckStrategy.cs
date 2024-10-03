namespace BerserkV3.GameCore.Cards
{
	public class CardDeckStrategy : BaseStrategy
	{
		public override ICardView View { get; set; }
		
		public CardDeckStrategy(ICardView cardView)
		{
			View = cardView;
		}

		protected override void OnEnabled()
		{
			View?.Layout.SetAlpha(0f);
			View?.Layout.SetInteractable(false);
			View?.GlowView?.Enable(false, GlowType.Turn);
		}
	}
}