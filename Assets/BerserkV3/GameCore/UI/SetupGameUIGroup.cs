using System;
using BerserkV3.Common.UIService;
using BerserkV3.Common.UIService.Abstractions;
using BerserkV3.Init.UI;
using RR.UIService;
using Zenject;

namespace BerserkV3.GameCore.UI
{
	namespace BerserkV3.Lobby.Home
	{
		public class SetupGameUIGroup : IInitializable, IDisposable
		{
			private readonly IUIService uiService;
			private readonly IUIAudioListener audioListener;

			public SetupGameUIGroup(
				IUIService uiService,
				IUIAudioListener audioListener)
			{
				this.uiService = uiService;
				this.audioListener = audioListener;
			}

			public void Initialize()
			{
				audioListener.Subscribe(uiService.CreateAll(UILayer.GameUIGroup));
				uiService.Begin<BackgroundWindow>().Hide(); // TODO fix future
			}

			public void Dispose()
			{
				uiService.DestroyAll(UILayer.LobbyUIGroup);
			}
		}
	}
}