using System;
using System.Collections.Generic;
using Berserk.Shared.Data.Game;
using Berserk.Shared.Data.Lobby;

namespace Berserk.Shared.Data.Identity
{
	public class UserDataModel
	{
		public string Id { get; set; }
		public string AccessToken { get; set; }
		public string RefreshToken { get; set; }
		public string UserName { get; set; }
		public string ReferralId { get; set; }
		public string LastDeckId { get; set; }
		public bool IsAcceptedPrivacyPolicy { get; set; }
		public bool IsAnonymous { get; set; }
		
		public DateTime? LastLogin { get; set; }
        public List<OwnedCard> OwnedCards { get; set; } = new();
		public List<DeckData> Decks { get; set; } = new();
		public List<OwnedVulcanite> OwnedVulcanites { get; set; } = new();
	}
}
