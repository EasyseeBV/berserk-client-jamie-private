using System.Collections.Generic;

namespace ServerCore.Infrastructure.Models
{
	public class PlayCardModel : SessionGameModel
	{
		public int HandCardsCount { get; set; }
		
		public int DeckCardsCount { get; set; }
		
		public List<InteractiveCardModel> Cards { get; set; } = new List<InteractiveCardModel>();
	}
}