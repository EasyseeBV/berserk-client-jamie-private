using System.Collections.Generic;
using Berserk.Shared.Data.Customisation;

namespace ServerCore.Infrastructure.Models
{
	public class SessionPlayerModel
	{
		public InteractiveCardModel NextDeckCard { get; set; }
		public int DeckCardsCount { get; set; }
		public int HandCardsCount { get; set; }
		public virtual HashSet<InteractiveCardModel> HandCards { get; set; } = new HashSet<InteractiveCardModel>();
		public virtual HashSet<InteractiveCardModel> TableCards { get; set; } = new HashSet<InteractiveCardModel>();
		public virtual HashSet<InteractiveCardModel> GraveCards { get; set; } = new HashSet<InteractiveCardModel>();

		public virtual InteractiveCardModel VulcaniteModel { get; set; }
		public virtual HashSet<CustomisationItemDto> UICustomizations { get; set; }

		public string Id { get; set; }
		public string UserName { get; set; }

		public int PlayerIndex { get; set; }
		public bool IsControlledByAI { get; set; }
		public PlayerConnectionStatus ConnectionStatus { get; set; }
		public bool IsMulliganReady { get; set; }
	}
}