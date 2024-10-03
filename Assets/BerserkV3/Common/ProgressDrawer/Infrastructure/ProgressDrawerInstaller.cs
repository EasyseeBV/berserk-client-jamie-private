using Zenject;

namespace BerserkV3.Common.ProgressDrawer
{
	public class ProgressDrawerInstaller : Installer<ProgressDrawerInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<ProgressDrawerApplication>()
				.AsSingle();
		}
	}
}