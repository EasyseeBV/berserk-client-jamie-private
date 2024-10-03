using Zenject;

namespace BerserkV3.Common.PreviewSystem
{
	public class PreviewSystemInstaller : Installer<PreviewSystemInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<PreviewSystem>()
				.AsSingle()
				.NonLazy();
			Container
				.Bind<PreviewSystemAdapter>()
				.AsSingle()
				.NonLazy();
		}
	}
}