using Berserk.Shared.Data.Lobby.Matchmaking.AutoMatching;

namespace BerserkV3.Lobby.MatchMaking.Leagues
{
	public static class LeaguesExtensions
	{
		public static string GetDescription(this LeagueModel @this)
		{
			return @this.LeagueDescription.Replace("\\n", "\n").Split('\n')[0].Trim();
		}

		public static string GetArtURL(this LeagueModel @this) // TODO move it to another place
		{
			return @this.LeagueDescription.Replace("\\n", "\n").Split('\n')[1].Trim();
		}
	}
}