using BerserkV3.Common.UIKit;
using RR.UIService;
using RR.UIService.AnimationSource;
using RR.UIService.AudioSource;
using RR.UIService.FullFade;
using RR.UIService.Options;
using Zenject;

namespace BerserkV3.Common.UIService.Infrastructure
{
	public class UIServiceInstaller : Installer<UIServiceInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<UIRoot>()
				.FromComponentInNewPrefabResource("UIRootCustom")  // TODO Removed AudioListener in prefab and disabled the EventSystem because it conflicts with the existing one in the project.
				.AsSingle()
				.NonLazy();

			Container
				.BindInterfacesTo<RR.UIService.UIService>()
				.AsSingle()
				.WithArguments(UILayer.DefaultUIGroup);

			Container
				.BindInterfacesTo<UIWindowPrototypeProvider>()
				.AsSingle()
				.WithArguments("");

			Container
				.BindInterfacesTo<BerserkUIWindowFactory>()
				.AsSingle();

			Container
				.BindInterfacesTo<UIAudioSourceFactory>()
				.AsSingle();

			Container
				.BindInterfacesTo<UIAudioHandlersFactory>()
				.AsSingle();

			Container
				.BindInterfacesTo<UIAnimationSourceFactory>()
				.AsSingle();

			Container
				.BindInterfacesTo<UIAnimationsFactory>()
				.AsSingle();

			Container
				.BindInterfacesTo<UIOptionsFactory>()
				.AsSingle();

			Container
				.BindInterfacesTo<UIServiceRepository>()
				.AsSingle();

			Container
				.BindInterfacesTo<UIFullFadePresenter>()
				.AsSingle();

			Container
				.BindInterfacesTo<UIAudioListener>()
				.AsSingle();

			Container
				.BindInterfacesTo<SetupDefaultUIGroup>()
				.AsSingle()
				.NonLazy();
			
			Container.BindInterfacesTo<UITimerController>()
				.AsSingle()
				.NonLazy();
		}
	}
}