using System;

namespace BerserkV3.Common.UIKit.KeyboardHeightService
{
	public class OtherPlatformsKeyboardProvider : IKeyboardHeightProvider
	{
		public float Height => 0;
		
		public void StartObserving(Action<float> keyboardHeightChanged){}
	}
}