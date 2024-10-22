using System.Collections.Generic;
using RR.UIService;

namespace BerserkV3.Common.UIService.Abstractions
{
	public interface IUIAudioListener
	{
		void Subscribe(IEnumerable<IUIWindow> windows);
	}
}