namespace BerserkV3.Lobby.LeaderBoard
{
	public class LeaderBoardApplicationAdapter
	{
		public static ILeaderBoardApplication Application { get; private set; }

		public LeaderBoardApplicationAdapter(ILeaderBoardApplication application)
		{
			Application = application;
		}
	}
}