using BerserkV3.GameCore.LocalStateValidator;
using BerserkV3.GameCore.LogicEventsProcessor;
using BerserkV3.GameCore.LogicEventsProcessor.BatchSystem;
using BerserkV3.GameCore.Network;
using Zenject;

namespace BerserkV3.GameCore.Infrastructure
{
	public class NetworkInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			Container.BindInterfacesTo<VisibleLocalState>()
				.AsSingle()
				.NonLazy();
			
			Container.BindInterfacesTo<LogicEventSignalBuffer>()
				.AsSingle()
				.NonLazy();

			Container.BindInterfacesTo<LogicEventsBatcher>()
				.AsSingle()
				.NonLazy();

			Container.BindInterfacesTo<GameLogicEventsProcessor>()
				.AsSingle()
				.NonLazy();

			Container
				.BindInterfacesTo<GameNetworkApplication>()
				.AsSingle()
				.NonLazy();

			Container
				.BindInterfacesTo<GameHub>()
				.AsSingle()
				.NonLazy();

			Container
				.BindInterfacesTo<LocalContextValidator>()
				.AsSingle()
				.NonLazy();
			
			Container.BindInterfacesTo<QueueOrderProcessor>()
				.AsSingle()
				.NonLazy();
		}
	}
}