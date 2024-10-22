using Zenject;

namespace BerserkV3.Common.AudioSystem.Infrastructure
{
	public class AudioSystemInstaller : Installer<AudioSystemInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<SimpleAudioPlayer>()
				.AsTransient();

			Container
				.BindInterfacesTo<BerserkAudioApplication>()
				.AsSingle()
				.NonLazy();
		}
	}
}