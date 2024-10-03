using Zenject;

namespace BerserkV3.Common.InputSystem.SelectionSystem
{
	public class SelectionSystemInstaller : Installer<SelectionSystemInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<SelectionSystem>()
				.AsSingle()
				.NonLazy();
		}
	}
}