using BerserkV3.GameCore.EffectsVisual.Applications;
using BerserkV3.GameCore.EffectsVisual.Collections;
using BerserkV3.GameCore.EffectsVisual.Factories;
using Zenject;

namespace BerserkV3.GameCore.Infrastructure
{
	public class EffectVisualsInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<AnimatorApplication>()
				.AsSingle()
				.NonLazy();

			Container
				.BindInterfacesTo<VisualEffectTypeCollection>()
				.AsSingle()
				.NonLazy();

			Container
				.BindInterfacesTo<VisualEffectsFactory>()
				.AsSingle()
				.NonLazy();

			Container
				.BindInterfacesTo<VisualSequenceApplication>()
				.AsSingle()
				.NonLazy();

			Container
				.BindInterfacesTo<VisualEffectsRepository>()
				.AsSingle()
				.NonLazy();

			Container
				.BindInterfacesTo<RuntimeEffectModelFactory>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<VisualConfigRepository>()
				.FromScriptableObjectResource(nameof(VisualConfigRepository))
				.AsSingle()
				.NonLazy();
		}
	}
}