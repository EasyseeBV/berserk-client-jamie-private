using Berserk.Shared.Lobby.Deck;
using Zenject;

namespace BerserkV3.Lobby.Deck
{
	public class DeckInstaller : Installer<DeckInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.Bind<DeckApplicationAdapter>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<DeckApplication>()
				.AsSingle()
				.NonLazy();
			
			Container
				.Bind<DeckValueApplicationAdapter>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<DeckValueService>()
				.AsSingle()
				.NonLazy();
		}
	}
}