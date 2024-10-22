namespace Berserk.Shared.Data.Lobby.Matchmaking.AutoMatching
{
    public class LeagueModel
    {
        public string Id { get; set; }
        public string LeagueName { get; set; }
        public string LeagueDescription { get; set; }
		
        public int MaxLevel { get; set; }
        public int MinLevel { get; set; }
        public int MaxTier { get; set; }
        public int MinTier { get; set; }

        public int MinFractionCards { get; set; }
		
        public bool IsIgnoreRestrictions { get; set; }
        public bool IsAllowed { get; set; }
		
        public string RequiredCardsIds { get; set; }
        public string BlackListCardIds { get; set; }
        public string BlackListVulcaniteIds { get; set; }
    }
}
