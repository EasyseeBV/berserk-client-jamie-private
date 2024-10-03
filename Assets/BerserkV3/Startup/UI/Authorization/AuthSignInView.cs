using System;
using BerserkV3.Common.UIKit;

namespace BerserkV3.Startup.UI
{
	public partial class AuthSignInView : SafeView
	{
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

		public string GetEmail()
		{
			return EmailInputField.GetValue();
		}

		public string GetPassword()
		{
			return PassInputField.GetValue();
		}

		public string GetTestPassword()
		{
			return TestPassInputField.GetValue();
		}

		public void SetActiveSeparator(bool value)
		{
			SetActive(SeparatorLayout, value);
		}
		
		public void SetHeaderText(string value)
		{
			HeaderText.SetText(value);
		}

		public void SetAuthText(string value)
		{
			AuthText.SetText(value);
		}
		
		public void SetEmailText(string value)
		{
			EmailInputField.SetText(value);
		}

		public void SetPasswordText(string value)
		{
			PassInputField.SetText(value);
		}

		public void SetTestPasswordText(string value)
		{
			TestPassInputField.SetText(value);
		}
		
		public void SetSignInButtonText(string value)
		{
			SignInButton.SetText(value);
		}

		public void SetSignUpButtonText(string value)
		{
			SignUpButton.SetText(value);
		}

		public void SetDeleteAccountButtonText(string value)
		{
			DeleteAccountButton.SetText(value);
		}

		public void SetGuestButtonText(string value)
		{
			GuestButton.SetText(value);
		}

		public void SetForgotPassButtonText(string value)
		{
			ForgotPassButton.SetText(value);
		}

		public void SetActiveTestPassword(bool value)
		{
			SetActive(TestPassInputField, value);
		}
		
		public void SetSignInAction(Action value)
		{
			SignInButton.Subscribe(value);
		}
		
		public void SetSignUpAction(Action value)
		{
			SignUpButton.Subscribe(value);
		}
		
		public void SetDeleteAccountAction(Action value)
		{
			DeleteAccountButton.Subscribe(value);
		}
		
		public void SetForgotPassAction(Action value)
		{
			ForgotPassButton.Subscribe(value);
		}
		
		public void SetGuestAction(Action value)
		{
			GuestButton.Subscribe(value);
		}

		private void Clear()
		{
			EmailInputField.Clear();
			PassInputField.Clear();
			TestPassInputField.Clear();
			SignInButton.Clear();
			SignUpButton.Clear();
			DeleteAccountButton.Clear();
			ForgotPassButton.Clear();
			GuestButton.Clear();
			SocialWidget.Clear();
		}
	}
}