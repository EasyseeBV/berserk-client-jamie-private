using System;
using BerserkV3.Common.UIService;
using BerserkV3.Common.UIService.Abstractions;
using BerserkV3.Init.UI;
using RR.UIService;
using Zenject;

namespace BerserkV3.Startup.Applications
{
	public class SetupStartupUIGroup : IInitializable, IDisposable
	{
		private readonly IUIService uiService;
		private readonly IUIAudioListener audioListener;

		public SetupStartupUIGroup(IUIService uiService, IUIAudioListener audioListener)
		{
			this.uiService = uiService;
			this.audioListener = audioListener;
		}

		public void Initialize()
		{
			audioListener.Subscribe(uiService.CreateAll(UILayer.StartupUIGroup));
			uiService.Begin<BackgroundWindow>()
				.WithMove(uiService.UIRoot.ButtomContainer, 0)
				.WithInit(window => window.SetDefaultImage())
				.Show();
		}

		public void Dispose()
		{
			uiService.DestroyAll(UILayer.StartupUIGroup);
		}
	}
}