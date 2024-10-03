using BerserkV3.Common.Network;
using BerserkV3.Startup.Network;
using RR.UI.FrameSystem;

namespace BerserkV3.Common.SystemDialogs
{
	public partial class ClientVersionView : BaseView
	{
		protected override void OnAwake()
		{
			base.OnAwake();
			RefreshVerion();
			EnvironmentSwitcher.OnSwitch += RefreshVerion;
		}

		private void OnDestroy()
		{
			EnvironmentSwitcher.OnSwitch -= RefreshVerion;
		}

		private void RefreshVerion()
		{
			VersionText.SetText(EnvironmentSwitcher.GetCurrentVersion());
		}
	}
}