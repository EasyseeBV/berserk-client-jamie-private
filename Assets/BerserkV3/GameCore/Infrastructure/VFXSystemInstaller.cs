using BerserkV3.GameCore.EffectsVisual.Applications;
using BerserkV3.GameCore.EffectsVisual.Factories;
using Zenject;

namespace BerserkV3.GameCore.Infrastructure
{
	public class VFXSystemInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<VfxFactory>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<VFXApplication>()
				.AsSingle()
				.NonLazy();
		}
	}
}