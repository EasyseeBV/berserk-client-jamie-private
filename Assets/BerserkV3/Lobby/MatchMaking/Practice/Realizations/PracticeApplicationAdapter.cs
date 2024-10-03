namespace BerserkV3.Lobby.MatchMaking.Practice
{
	public class PracticeApplicationAdapter
	{
		public static IPracticeApplication Application { get; private set; }
		public PracticeApplicationAdapter(IPracticeApplication application)
		{
			Application = application;
		}
	}
}