using System;

namespace BerserkV3.Startup.UI
{
	public partial class AuthResetPassView : SafeView
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
		
		public string GetCodeInputText()
		{
			return CodeInputField.GetValue();
		}
		
		public string GetPassInputText()
		{
			return PassInputField.GetValue();
		}
		
		public void SetHeaderText(string value)
		{
			Set(HeaderText, value);
		}
		
		public void SetMessageText(string value)
		{
			Set(MessageText, value);
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
		
		public void SetCodeInputText(string value)
		{
			CodeInputField.SetText(value);
		}

		public void SetPassInputText(string value)
		{
			PassInputField.SetText(value);
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
		
		public void SetCodeInputValueChangeAction(Action<string> value)
		{
			CodeInputField.SetValueChangeAction(value);
		}
		
		public void SetPassInputValueChangeAction(Action<string> value)
		{
			PassInputField.SetValueChangeAction(value);
		}
		
		private void Clear()
		{
			CodeInputField.Clear();
			PassInputField.Clear();
			FooterButton.Clear();
			ResetButton.Clear();
			ReturnButton.Clear();
		}
	}
}