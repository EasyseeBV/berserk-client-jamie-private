using System;

namespace BerserkV3.Startup.UI
{
	public partial class AuthForgotPassView : SafeView
	{
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

		public string GetInputText()
		{
			return InputField.GetValue();
		}

		public void SetHeaderText(string value)
		{
			Set(HeaderText, value);
		}

		public void SetFieldText(string value)
		{
			Set(FieldText, value);
		}

		public void SetResetText(string value)
		{
			ResetButton.SetText(value);
		}

		public void SetResetInteractable(bool value)
		{
			ResetButton.SetInteractable(value);
		}

		public void SetFooterText(string value)
		{
			FooterButton.SetText(value);
		}

		public void SetInputText(string value)
		{
			InputField.SetText(value);
		}

		public void SetFooterAction(Action value)
		{
			FooterButton.Subscribe(value);
		}

		public void SetReturnAction(Action value)
		{
			ReturnButton.Subscribe(value);
		}

		public void SetResetAction(Action value)
		{
			ResetButton.Subscribe(value);
		}

		public void SetInputValueChangeAction(Action<string> value)
		{
			InputField.SetValueChangeAction(value);
		}

		private void Clear()
		{
			InputField.Clear();
			FooterButton.Clear();
			ResetButton.Clear();
			ReturnButton.Clear();
		}
	}
}