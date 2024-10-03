using BerserkV3.Generic.Emotions;
using Zenject;

namespace BerserkV3.GameCore.Emotions
{
	public class GameEmotionsInstaller : Installer<GameEmotionsInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<GameEmotionApplication>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<EmotionsViewFactory>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<EmotionsFactory>()
				.AsSingle()
				.NonLazy();
			
			Container
				.BindInterfacesTo<EmotionsRepository>()
				.AsSingle()
				.NonLazy();
		}
	}
}