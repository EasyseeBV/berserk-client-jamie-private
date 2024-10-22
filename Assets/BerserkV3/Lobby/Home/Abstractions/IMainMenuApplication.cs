namespace BerserkV3.Lobby.Home.Abstractions
{
	public interface IMainMenuApplication
	{
		public void Init();
		
		public void Redirect(params object[] args);
	}
}