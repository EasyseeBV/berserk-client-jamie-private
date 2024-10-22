namespace Berserk.Shared.Data
{
	public class StoreConfig
	{
		public bool MarketplaceEnabled { get; set; }
		public bool SyncOnlyWeb3Assets { get; set; }
		public bool ScheduleFailedStoreApiCalls { get; set; }
		public bool ExecuteScheduledStoreApiCalls { get; set; }
		public bool ScheduleFailedLeaderboardApiCalls { get; set; }
		public bool ExecuteScheduledLeaderboardApiCalls { get; set; }
		public string MarketplaceUrl { get; set; }
		public string MarketplaceWelcomePackUrl { get; set; }
		public string MarketplaceCollectionUrl { get; set; }
		public string MarketplaceDeckTicketPackUrl { get; set; }
		public string MarketplaceCurrencyUrl { get; set; }
		public string FrontendUrl { get; set; }
		public string TournamentLeaderboardUrl { get; set; }
		public string HardCurrencyTabId { get; set; }

		public StoreRedirectData RedirectWelcomePack { get; set; }
		public StoreRedirectData RedirectDeckTicket { get; set; }
	}
}
