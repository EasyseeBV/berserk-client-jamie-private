using System.Collections.Generic;
using BerserkV3.Common.AudioSystem.Abstractions;
using BerserkV3.Common.UIService.Abstractions;
using RR.Core.Extensions;
using RR.UIService;

namespace BerserkV3.Common.UIService
{
	public class UIAudioListener : IUIAudioListener
	{
		private readonly IAudioApplication audioApplication;
		public UIAudioListener (IAudioApplication audioApplication)
		{
			this.audioApplication = audioApplication;
		}

		public void Subscribe(IEnumerable<IUIWindow> windows)
		{
			windows?.ForEach(x => x.AudioSource.OnPlayAudio += data => audioApplication.PlaySound(data));
		}
	}
}