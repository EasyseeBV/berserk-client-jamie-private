namespace BerserkV3.GameCore.Cards
{
	public class CardInShowAllStrategy : BaseStrategy
	{
		private static float CardSize => 0.8f;
		public override ICardView View { get; set; }

		public CardInShowAllStrategy(ICardView cardView)
		{
			View = cardView;
		}

		protected override void OnEnabled()
		{
			View?.Layout.SetAlpha(1f);
			View?.SetSize(CardSize);
			View?.Layout.SetInteractable(false);
			View?.GlowView?.Enable(false, GlowType.Turn);
		}
	}
}