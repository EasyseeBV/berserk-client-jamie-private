using Berserk.Shared.GameCore.EffectSystem;
using BerserkV3.GameCore.EffectsVisual.Applications;
using BerserkV3.GameCore.EffectsVisual.Factories;
using Zenject;

namespace BerserkV3.ShowRoom.VfxShowRoom
{
	public class VfxShowRoomInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<VfxFactory>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<EffectsFactory>()
				.AsSingle()
				.WithArguments(true)
				.NonLazy();
			
			Container
				.BindInterfacesTo<VFXApplication>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<VfxGameContainers>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<ShowRoomApplication>()
				.AsSingle()
				.NonLazy();
		}
	}
}