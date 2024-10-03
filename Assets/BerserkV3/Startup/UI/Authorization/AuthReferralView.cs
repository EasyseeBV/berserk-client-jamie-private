using System;

namespace BerserkV3.Startup.UI
{
	public partial class AuthReferralView : SafeView
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
		
		public void SetHeaderText(string value)
		{
			Set(HeaderText, value);
		}
		
		public void SetMessageText(string value)
		{
			Set(MessageText, value);
		}
		
		public void SetInputText(string value)
		{
			InputField.SetText(value);
		}
		
		public void SetSkipText(string value)
		{
			SkipButton.SetText(value);
		}
		
		public void SetSubmitText(string value)
		{
			SubmitButton.SetText(value);
		}
		
		public string GetInputText()
		{
			return InputField.GetValue();
		}
		
		public void SetInputChangeAction(Action<string> action)
		{
			InputField.SetValueChangeAction(action);
		}
		
		public void SetSubmitAction(Action value)
		{
			SubmitButton.Subscribe(value);
		}
		
		public void SetSkipAction(Action value)
		{
			SkipButton.Subscribe(value);
		}
		
		public void SetSubmitInteractable(bool value)
		{
			SubmitButton.SetInteractable(value);
		}
		
		private void Clear()
		{
			SubmitButton.Clear();
			SkipButton.Clear();
			InputField.Clear();
		}
	}
}