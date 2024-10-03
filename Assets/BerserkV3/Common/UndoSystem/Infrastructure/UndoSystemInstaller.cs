using Zenject;

namespace BerserkV3.Generic.UndoSystem
{
	public class UndoSystemInstaller : Installer<UndoSystemInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<UndoSystem>()
				.AsSingle();
		}
	}
}