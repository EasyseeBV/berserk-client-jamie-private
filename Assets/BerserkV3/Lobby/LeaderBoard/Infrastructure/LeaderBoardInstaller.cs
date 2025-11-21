using Zenject;

namespace BerserkV3.Lobby.LeaderBoard
{
	public class LeaderBoardInstaller : Installer<LeaderBoardInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.Bind<LeaderBoardApplicationAdapter>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<LeaderBoardApplication>()
				.AsSingle()
				.NonLazy();
		}
	}
}