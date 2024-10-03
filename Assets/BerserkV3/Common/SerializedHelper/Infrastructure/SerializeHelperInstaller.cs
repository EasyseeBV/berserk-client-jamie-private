using Zenject;

namespace BerserkV3.Common.SerializedHelper
{
	public class SerializeHelperInstaller : Installer<SerializeHelperInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<LocalSerilizeHelper>()
				.AsSingle()
				.NonLazy();
			
			Container
				.Bind<SerializeHelperAdapter>()
				.AsSingle()
				.NonLazy();
		}
	}
}