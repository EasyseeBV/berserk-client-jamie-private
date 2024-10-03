using System;
using UnityEngine;

namespace BerserkV3.Common.UIKit.KeyboardHeightService
{
	public class AndroidKeyboardHeightProvider : IKeyboardHeightProvider, IDisposable
	{
		private readonly AndroidJavaObject decorView;
		private readonly AndroidJavaObject rect;
		private float lastCalculatedHeight;
		private event Action<float> OnHeightChanged;
		
		public float Height => GetHeight();

		public AndroidKeyboardHeightProvider()
		{
			var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			decorView = activity.Call<AndroidJavaObject>("getWindow").Call<AndroidJavaObject>("getDecorView");
			rect = new AndroidJavaObject("android.graphics.Rect");
		}

		public void Dispose()
		{
			OnHeightChanged = null;
			decorView?.Dispose();
			rect?.Dispose();
		}

		public void StartObserving(Action<float> keyboardHeightChanged)
		{
			OnHeightChanged += height => keyboardHeightChanged?.Invoke(height);
		}

		private float GetHeight()
		{
			if (decorView == null || rect == null)
				return lastCalculatedHeight = 0;

			decorView.Call("getWindowVisibleDisplayFrame", rect);

			var screenHeight = Screen.height;
			var rectHeight = rect.Call<int>(nameof(Rect.height));
			var keyboardHeigh = screenHeight - rectHeight;
			var lastValue = lastCalculatedHeight;
			lastCalculatedHeight = keyboardHeigh;
			
			if (Math.Abs(keyboardHeigh - lastValue) > 0.1f)
				OnHeightChanged?.Invoke(lastCalculatedHeight);

			return lastCalculatedHeight;
		}
	}
}