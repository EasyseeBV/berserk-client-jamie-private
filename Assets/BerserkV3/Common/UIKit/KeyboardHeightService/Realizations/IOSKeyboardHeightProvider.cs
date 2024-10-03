using System;
using UnityEngine;

namespace BerserkV3.Common.UIKit.KeyboardHeightService
{
	public class IOSKeyboardHeightProvider : IKeyboardHeightProvider, IDisposable
	{
		private event Action<float> OnHeightChanged;
		private float lastCalculatedHeight;
	
		public float Height => GetHeight();

		public void Dispose()
		{
			OnHeightChanged = null;
		}

		private void KeyboardHeightChanged(float height)
		{
			OnHeightChanged?.Invoke(height);
		}

		public void StartObserving(Action<float> onHeightChanged)
		{
			OnHeightChanged += height => onHeightChanged?.Invoke(height);
		}
	
		private float GetHeight()
		{
			var keyboardHeight = TouchScreenKeyboard.area.height;
			var lastHegiht = lastCalculatedHeight;
			lastCalculatedHeight = keyboardHeight;
			
			if (Math.Abs(keyboardHeight - lastHegiht) > 0.1f)
				KeyboardHeightChanged(keyboardHeight);


			return keyboardHeight;
		}
	}
}