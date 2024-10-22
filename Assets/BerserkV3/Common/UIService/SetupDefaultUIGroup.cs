using BerserkV3.Common.UIService.Abstractions;
using BerserkV3.Init.UI;
using RR.UIService;
using Zenject;

namespace BerserkV3.Common.UIService
{
	public class SetupDefaultUIGroup : IInitializable
	{
		private readonly IUIService uiService;
		private readonly IUIAudioListener audioListener;

		public SetupDefaultUIGroup(IUIService uiService, IUIAudioListener audioListener)
		{
			this.uiService = uiService;
			this.audioListener = audioListener;
		}

		public void Initialize()
		{
			audioListener.Subscribe(uiService.CreateAll(UILayer.DefaultUIGroup));
			uiService.Begin<BackgroundWindow>()
				.WithMove(uiService.UIRoot.ButtomContainer, 0)
				.WithInit(window => window.SetDefaultImage())
				.Show();
		}
	}
}