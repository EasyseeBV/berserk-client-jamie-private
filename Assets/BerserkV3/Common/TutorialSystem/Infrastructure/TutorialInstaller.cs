using RR.Game.TutorialSystemV2.Realizations;
using Zenject;

namespace BerserkV3.Common.TutorialSystem
{
    public class TutorialInstaller : Installer<TutorialInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .BindInterfacesTo<BerserkTutorialApplication>()
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesTo<RemoteTutorialEntitiesRepository>()
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesTo<DefaultTutorialHandlersRepository>()
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesTo<DefaultTutorialTextFormatter>()
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesTo<RemoteTutorialProgressRepository>()
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesTo<DefaultTutorialCameraProvider>()
                .AsSingle()
                .NonLazy();
            
            Container
                .Bind<TutorialAdapter>()
                .AsSingle()
                .NonLazy();
        }
    }
}