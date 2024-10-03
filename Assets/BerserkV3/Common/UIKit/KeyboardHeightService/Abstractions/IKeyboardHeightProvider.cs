using System;

namespace BerserkV3.Common.UIKit.KeyboardHeightService
{
	public interface IKeyboardHeightProvider
	{
		float Height { get; }
		void StartObserving(Action<float> keyboardHeightChanged);
	}
}