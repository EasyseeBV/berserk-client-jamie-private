using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.GameCore.SharedImplementations;
using Zenject;

namespace BerserkV3.GameCore.Infrastructure
{
	public class SharedInstaller : Installer<SharedInstaller>
	{
		public override void InstallBindings()
		{
			InstallGameContext();
			InstallClientImplementation();
		}

		private void InstallGameContext()
		{
			Container.BindInterfacesTo<GiveCardsServiceFactory>().AsSingle().NonLazy();
			Container.BindInterfacesTo<RuntimeIdGenerator>().AsSingle().NonLazy();
			Container.BindInterfacesTo<RuntimeRandomGenerator>().AsSingle().NonLazy();
			Container.BindInterfacesTo<SharedEventSource>().AsSingle().NonLazy();
			Container.BindInterfacesTo<PlayerRepository>().AsSingle().NonLazy();
			Container.BindInterfacesTo<OrderGenerator>().AsSingle().NonLazy();
			Container.BindInterfacesTo<RuntimeTimer>().AsSingle().NonLazy();
			Container.BindInterfacesTo<RuntimePool>().AsSingle().NonLazy();
			Container.BindInterfacesTo<GameContext>().AsSingle().NonLazy();
		}

		private void InstallClientImplementation()
		{
			Container.BindInterfacesTo<ClientSessionProcessor>().AsSingle().NonLazy();
			Container.BindInterfacesTo<ClientGameLogicContext>().AsSingle().NonLazy();
		}
	}
}