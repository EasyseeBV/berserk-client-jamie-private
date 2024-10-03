using Zenject;

namespace BerserkV3.Common.InputSystem.DragDropSystem
{
	public class DragSystemInstaller : Installer<DragSystemInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<DragDropSystem>()
				.AsSingle()
				.NonLazy();

			Container
				.BindInterfacesTo<DragDropInputHandler>()
				.AsSingle()
				.NonLazy();
		}
	}
}