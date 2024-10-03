using Zenject;

namespace BerserkV3.GameCore.SplineSystem
{
	public class SplineSystemInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<SplineStorage>()
				.AsSingle()
				.NonLazy();
		}
	}
}