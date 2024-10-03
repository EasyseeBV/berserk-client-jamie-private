using Zenject;

namespace BerserkV3.Lobby.MatchMaking.Practice
{
	public class PracticeInstaller : Installer<PracticeInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<PracticeApplication>()
				.AsSingle()
				.NonLazy();
			
			Container
				.Bind<PracticeApplicationAdapter>()
				.AsSingle()
				.NonLazy();
		}
	}
}