using BerserkV3.Common.UIKit.NotifyService.Controllers;
using Zenject;

namespace BerserkV3.Common.UIKit.NotifyService.Infrastructure
{
	public class NotifyServiceInstaller : Installer<NotifyServiceInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<Realizations.NotifyService>()
				.AsSingle();
		}
	}
}