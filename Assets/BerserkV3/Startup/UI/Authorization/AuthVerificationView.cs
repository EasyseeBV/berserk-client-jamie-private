using System;
using BerserkV3.Common.UIKit;

namespace BerserkV3.Startup.UI
{
	public partial class AuthVerificationView : SafeView
	{
		public TimerWidget TimerWidget => timerWidget;

		protected override void OnHidden()
		{
			base.OnHidden();
			Clear();
		}

		protected override void OnClosed()
		{
			base.OnClosed();
			Clear();
		}

		private void OnDestroy()
		{
			Clear();
		}

		public string GetInput()
		{
			return InputField.GetValue();
		}

		public void SetInputText(string value)
		{
			InputField.SetText(value);
		}

		public void SetHeaderText(string value)
		{
			Set(HeaderText, value);
		}

		public void SetMessageText(string value)
		{
			Set(MessageText, value);
		}

		public void SetSubmitText(string value)
		{
			SubmitButton.SetText(value);
		}

		public void SetFooterText(string value)
		{
			FooterButton.SetText(value);
		}

		public void SetFooterInteractable(bool value)
		{
			FooterButton.SetInteractable(value);
		}

		public void SetSubmitInteractable(bool value)
		{
			SubmitButton.SetInteractable(value);
		}

		public void SetInputChangedAction(Action<string> value)
		{
			InputField.SetValueChangeAction(value);
		}
		
		public void SetSubminAction(Action value)
		{
			SubmitButton.Subscribe(value);
		}

		public void SetFooterAction(Action value)
		{
			FooterButton.Subscribe(value);
		}

		public void SetReturnAction(Action value)
		{
			ReturnButton.Subscribe(value);
		}

		private void Clear()
		{
			FooterButton.Clear();
			SubmitButton.Clear();
			ReturnButton.Clear();
			InputField.Clear();
		}
	}
}