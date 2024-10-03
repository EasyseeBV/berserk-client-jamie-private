using System;
using Object = UnityEngine.Object;

namespace BerserkV3.Common.UIKit.KeyboardHeightService
{
	public interface IKeyboardHeightService
	{
		event Action<Object> OnFocuseChanged;
		float KeyboardHeight { get; }
		Object Focused { get; }

		void SetFocuse(Object state);
		void Release(Object state);
	}
}