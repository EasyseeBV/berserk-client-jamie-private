using Zenject;

namespace BerserkV3.GameCore.TargetSystem.Infrastructure
{
	public class TargetSystemInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<ManualArrowSystem>()
				.AsSingle()
				.NonLazy();
		}
	}
}