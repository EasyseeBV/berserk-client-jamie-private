using System;
using BerserkV3.Common.UIService;
using BerserkV3.Common.UIService.Abstractions;
using RR.UIService;
using Zenject;

namespace BerserkV3.Init.Applications
{
	public class SetupInitUIGroup : IInitializable, IDisposable
	{
		private readonly IUIService uiService;
		private readonly IUIAudioListener audioListener;

		public SetupInitUIGroup(IUIService uiService, IUIAudioListener audioListener)
		{
			this.uiService = uiService;
			this.audioListener = audioListener;
		}

		public void Initialize()
		{
			audioListener.Subscribe(uiService.CreateAll(UILayer.InitUIGroup));
		}

		public void Dispose()
		{
			uiService.DestroyAll(UILayer.InitUIGroup);
		}
	}
}