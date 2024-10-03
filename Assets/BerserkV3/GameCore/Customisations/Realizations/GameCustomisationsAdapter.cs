namespace BerserkV3.GameCore.Customisations
{
	public class GameCustomisationsAdapter
	{
		public static IGameCustomisationApplication Application { get; private set; }
		
		public GameCustomisationsAdapter(IGameCustomisationApplication application)
		{
			Application = application;
		}
	}
}