using Zenject;

namespace BerserkV3.Common.AppTime
{

	public class AppTimeInstaller : Installer<AppTimeInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<AppTimeSynchronizer>()
				.AsSingle()
				.NonLazy();
		}
	}

}