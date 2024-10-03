using System;
using BerserkV3.Common.UIKit;

namespace BerserkV3.Startup.UI
{
	public partial class AuthSignUpGuestView : SafeView
	{
		protected override int InteractableDelayMs => 1000;
		public SocialWidget SocialWidget => SocialLayout;
		protected override void OnClosed()
		{
			base.OnClosed();
			Clear();
		}
		
		protected override void OnHidden()
		{
			base.OnHidden();
			Clear();
		}
		
		private void OnDestroy()
		{
			Clear();
		}
		
		public string GetUserName()
		{
			return UserNameInputField.GetValue();
		}
		
		public string GetEmail()
		{
			return EmailInputField.GetValue();
		}
		
		public string GetPassword()
		{
			return PassInputField.GetValue();
		}
		
		public void SetActiveSeparator(bool value)
		{
			SetActive(SeparateLayout, value);
		}
		
		public void SetEmailInputText(string value)
		{
			EmailInputField.SetText(value);
		}
		
		public void SetUserNameInputText(string value)
		{
			UserNameInputField.SetText(value);
		}
		
		public void SetPasswordInputText(string value)
		{
			PassInputField.SetText(value);
		}
		
		public void SetHeaderText(string value)
		{
			HeaderText.SetText(value);
		}
		
		public void SetLoginButtonText(string value)
		{
			LoginButton.SetText(value);
		}
		
		public void SetGuestButtonText(string value)
		{
			GuestButton.SetText(value);
		}
		
		public void SetContinueButtonText(string value)
		{
			ContinueButton.SetText(value);
		}
		
		public void SetContinueAction(Action value)
		{
			ContinueButton.Subscribe(value);
		}
		
		public void SetGuestAction(Action value)
		{
			GuestButton.Subscribe(value);
		}
		
		public void SetCancelAction(Action value)
		{
			CancelButton.Subscribe(value);
		}
		
		public void SetLoginAction(Action value)
		{
			LoginButton.Subscribe(value);
		}
		
		private void Clear()
		{
			SocialWidget.Clear();
			ContinueButton.Clear();
			GuestButton.Clear();
			LoginButton.Clear();
			UserNameInputField.Clear();
			EmailInputField.Clear();
			PassInputField.Clear();
		}
	}
}