using Zenject;

namespace BerserkV3.GameCore.TooltipPopup.Infrastructure
{
	public class TooltipPopupInstaller : MonoInstaller<TooltipPopupInstaller>
	{
		public override void InstallBindings()
		{
			Container
				.BindInterfacesTo<TooltipPopupDoubleSidedController>()
				.AsSingle()
				.NonLazy();
		}
	}
}