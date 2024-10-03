using Zenject;

namespace BerserkV3.Generic.SharedLogger
{
	public class SharedLoggerInstaller : Installer<SharedLoggerInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<RRSharedLogger>()
				.AsSingle()
				.NonLazy();
		}
	}
}