using System;
using Events;
using RR.UI.FrameSystem;
using UnityEngine;

namespace UI
{
	public partial class ErrorDialog : BaseView
	{
		protected override void OnAwake()
		{
			ErrorDispatcher.OnError.Subscribe(this, ShowError).CallWhenInactive();
			ErrorDispatcher.OnException.Subscribe(this, ShowException).CallWhenInactive();
			
			OKButton.Subscribe(Close);
			CopyButton.Subscribe(Copy);
		}

		private void ShowError(string error)
		{
			Set(ErrorText, error);
			Show();
		}

		private void ShowError(int code, string error)
		{
			ShowError($"Code: {code}, Message: {error}");
		}

		private void ShowException(Exception e)
		{
			Set(ErrorText, e.Message);
			Show();
		}

		private void Copy()
		{
			GUIUtility.systemCopyBuffer = ErrorText.text;
			WebGLCopyAndPasteAPI.PassCopyToBrowser(ErrorText.text);
		}
	}
}