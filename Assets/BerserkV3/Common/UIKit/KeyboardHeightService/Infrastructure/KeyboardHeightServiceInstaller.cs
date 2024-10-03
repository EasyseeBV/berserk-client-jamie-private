using UnityEngine;
using Zenject;
using Application = UnityEngine.Device.Application;

namespace BerserkV3.Common.UIKit.KeyboardHeightService
{
	public class KeyboardHeightServiceInstaller : Installer<KeyboardHeightServiceInstaller>
	{
		public override void InstallBindings()
		{
			BindKeyboardHeightProvider();
			
			Container
				.BindInterfacesTo<KeyboardHeightService>()
				.AsSingle()
				.NonLazy();
			
			Container
				.Bind<KeyboardHeightServiceAdapter>()
				.AsSingle()
				.NonLazy();
		}

		private void BindKeyboardHeightProvider()
		{
			switch (Application.platform)
			{
				case RuntimePlatform.Android : 			
					Container
						.BindInterfacesTo<AndroidKeyboardHeightProvider>()
						.AsSingle()
						.NonLazy();
					return;
				
				case RuntimePlatform.IPhonePlayer or RuntimePlatform.tvOS or RuntimePlatform.OSXPlayer: 
					Container
						.BindInterfacesTo<IOSKeyboardHeightProvider>()
						.AsSingle()
						.NonLazy();
					return;
				
				default:
					Container
						.BindInterfacesTo<OtherPlatformsKeyboardProvider>()
						.AsSingle()
						.NonLazy();
					return;
			}
		}
	}
}