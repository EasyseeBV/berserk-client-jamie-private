using BerserkV3.Common.ProgressDrawer;
using BerserkV3.GameCore.UI;
using UnityEngine;
using Zenject;

namespace BerserkV3.Generic.SystemDialogs
{
	public class SystemDialogsInstaller : Installer<SystemDialogsInstaller>
	{
		public override void InstallBindings()
		{
			var systemDialogsRoot = Object.FindObjectOfType<SystemDialogsRoot>();
			Container
				.Bind<IReconnectionView>()
				.FromInstance(systemDialogsRoot.ReconnectionView)
				.AsSingle();
			
			Container
				.Bind<IProgressDrawerView>()
				.FromInstance(systemDialogsRoot.ProgressDrawerView)
				.AsSingle();
		}
	}
}