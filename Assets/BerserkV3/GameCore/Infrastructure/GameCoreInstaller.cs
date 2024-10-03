using BerserkV3.Common.Utils;
using BerserkV3.GameCore.Cards;
using BerserkV3.GameCore.Cards.EffectHints;
using BerserkV3.GameCore.Controllers;
using BerserkV3.GameCore.Controllers.Graveyard;
using BerserkV3.GameCore.Customisations;
using BerserkV3.GameCore.EffectsVisual.Applications;
using BerserkV3.GameCore.Emotions;
using BerserkV3.GameCore.Prediction.Realizations;
using BerserkV3.GameCore.Repository;
using BerserkV3.GameCore.Settings;
using BerserkV3.Startup.Applications;
using Zenject;

namespace BerserkV3.GameCore.Infrastructure
{
	public class GameCoreInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			GameCustomisationInstaller.Install(Container);
			GameEmotionsInstaller.Install(Container);
			SharedInstaller.Install(Container);
			
			Container.BindInterfacesTo<GuestApplication>()
				.AsSingle()
				.NonLazy();
			
			Container.BindInterfacesTo<SoundController>()
				.AsSingle()
				.NonLazy();
			
			Container.BindInterfacesTo<EffectHintsApplication>()
				.AsTransient();

			Container.BindInterfacesTo<GameViewController>()
				.AsSingle()
				.NonLazy();

			Container.BindInterfacesTo<GraveyardController>()
				.AsSingle()
				.NonLazy();
			
			Container.BindInterfacesTo<TimerController>()
				.AsSingle()
				.NonLazy();
			
			Container.BindInterfacesTo<DeckController>()
				.AsSingle()
				.NonLazy();

			Container.BindInterfacesTo<GameLocks>()
				.AsSingle()
				.NonLazy();
			
			Container.BindInterfacesTo<GameContainers>()
				.AsSingle()
				.NonLazy();
					
			Container.BindInterfacesTo<ReportService>()
				.AsSingle()
				.NonLazy();

			Container.BindInterfacesTo<GameRepository>()
				.AsSingle()
				.NonLazy();

			Container.BindInterfacesTo<DebugController>()
				.AsSingle()
				.NonLazy();

			Container.BindInterfacesTo<TableController>()
				.AsSingle()
				.NonLazy();

			Container.BindInterfacesTo<GameEndController>()
				.AsSingle()
				.NonLazy();

			Container.BindInterfacesTo<GameSettingsApplication>()
				.AsSingle()
				.NonLazy();

			Container.BindInterfacesTo<StartupController>()
				.AsSingle()
				.NonLazy();

			Container.BindInterfacesTo<SimpleActionsQueue>()
				.AsSingle()
				.NonLazy();
			
			Container.BindInterfacesTo<EffectsApplication>()
				.AsSingle()
				.NonLazy();

			Container.BindInterfacesTo<CardsStateController>()
				.AsSingle()
				.NonLazy();

			Container.BindInterfacesTo<OpponentHandController>()
				.AsSingle()
				.NonLazy();

			Container.BindInterfacesTo<InvalidActionController>()
				.AsSingle()
				.NonLazy();

			Container.BindInterfacesTo<PredictProcessor>()
				.AsSingle()
				.NonLazy();
		}
	}
}