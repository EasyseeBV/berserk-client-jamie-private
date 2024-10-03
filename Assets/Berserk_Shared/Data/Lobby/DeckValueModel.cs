using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.Data.Lobby
{
	public abstract class DeckValueModel {}

	public class DeckRarityValueModel : DeckValueModel
	{
		public Rarity Rarity { get; set; }
		public float Value { get; set; }
	}

	public class DeckNftBonusModel : DeckValueModel
	{
		public float Value { get; set; }
		public int MaxRange { get; set; }
		public int MinRange { get; set; }
	}
}