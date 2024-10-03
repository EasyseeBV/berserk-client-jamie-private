using BerserkV3.Common.PreviewSystem;
using BerserkV3.GameCore.Cards;
using Zenject;

namespace BerserkV3.GameCore.Infrastructure
{
	public class CardsInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			PreviewSystemInstaller.Install(Container);
			BindFactories();
			BindPositioning();
		}

		private void BindFactories()
		{
			Container
				.BindInterfacesTo<CardViewFactory>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<OpponentHandViewFactory>()
				.AsSingle()
				.NonLazy();

			Container
				.BindInterfacesTo<CardStrategyFactory>()
				.AsSingle()
				.NonLazy();

			Container
				.BindInterfacesTo<HeroViewFactory>()
				.AsSingle()
				.NonLazy();
		}

		private void BindPositioning()
		{
			Container.BindInterfacesTo<SelfHandCardsPositioning>()
				.AsSingle()
				.NonLazy();


			Container.BindInterfacesTo<OpponentHandCardsPositioning>()
				.AsSingle()
				.NonLazy();


			Container.BindInterfacesTo<HorizontalPositioning>()
				.AsSingle()
				.NonLazy();


			Container.BindInterfacesTo<GridPositioning>()
				.AsSingle()
				.NonLazy();
			
			
			Container
				.BindInterfacesTo<TableCardsPositioning>()
				.AsSingle()
				.NonLazy();
		}
	}
}