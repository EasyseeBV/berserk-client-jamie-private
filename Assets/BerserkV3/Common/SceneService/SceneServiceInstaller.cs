using Zenject;

namespace BerserkV3.Common.SceneService
{
	public class SceneServiceInstaller : Installer<SceneServiceInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<SceneService>()
				.AsSingle()
				.NonLazy();
			
			Container
				.Bind<SceneServiceAdapter>()
				.AsSingle()
				.NonLazy();

		}
	}
}