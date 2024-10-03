using System;
using Object = UnityEngine.Object;

namespace BerserkV3.Common.UIKit.KeyboardHeightService
{
	public class KeyboardHeightService : IKeyboardHeightService, IDisposable
	{
		private readonly IKeyboardHeightProvider keyboardHeightProvider;
		public float KeyboardHeight => keyboardHeightProvider.Height;
		public event Action<Object> OnFocuseChanged;

		public Object Focused { get; private set; }

		public KeyboardHeightService(IKeyboardHeightProvider keyboardHeightProvider)
		{
			this.keyboardHeightProvider = keyboardHeightProvider;
		}

		public void SetFocuse(Object state)
		{
			if (Focused == state)
				return;
			
			Release(Focused);
			Focused = state;
			OnFocuseChanged?.Invoke(state);
		}

		public void Release(Object state)
		{
			if (!Focused || Focused != state)
				return;

			Focused = null;
			OnFocuseChanged?.Invoke(state);
		}

		public void Dispose()
		{
			OnFocuseChanged = null;
		}
	}
}