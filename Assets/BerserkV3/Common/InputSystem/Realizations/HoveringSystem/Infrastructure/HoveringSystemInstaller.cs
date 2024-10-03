using Zenject;

namespace BerserkV3.Common.InputSystem.HoveringSystem
{
	public class HoveringSystemInstaller : Installer<HoveringSystemInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<HoveringSystem>()
				.AsSingle()
				.NonLazy();
		}
	}
}