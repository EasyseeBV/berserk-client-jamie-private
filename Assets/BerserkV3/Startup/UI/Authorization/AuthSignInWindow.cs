using System;
using BerserkV3.Common.UIKit;
using BerserkV3.Common.UIService;
using TMPro;
using UnityEngine;

namespace BerserkV3.Startup.UI
{
	public class AuthSignInWindow : UISafeWindowBase
	{
		[SerializeField] protected TextMeshProUGUI HeaderText;
		[SerializeField] protected TextMeshProUGUI AuthText;
		[SerializeField] protected SocialWidget SocialLayout;
		[SerializeField] protected CanvasGroup SeparatorLayout;
		[SerializeField] protected ComplexInputField EmailInputField;
		[SerializeField] protected ComplexInputField PassInputField;
		[SerializeField] protected ComplexInputField TestPassInputField;
		[SerializeField] protected ComplexButton SignInButton;
		[SerializeField] protected ComplexButton GuestButton;
		[SerializeField] protected ComplexButton SignUpButton;
		[SerializeField] protected ComplexButton DeleteAccountButton;
		[SerializeField] protected ComplexButton ForgotPassButton;

		public SocialWidget SocialWidget => SocialLayout;

		public override void Hidden()
		{
			Clear();
			base.Hidden();
		}

		protected override void OnDisposed()
		{
			Clear();
			base.OnDisposed();
		}

		public string GetEmail()
		{
			return !EmailInputField ? null : EmailInputField.GetValue();
		}

		public string GetPassword()
		{
			return !PassInputField ? null : PassInputField.GetValue();
		}

		public string GetTestPassword()
		{
			return !TestPassInputField ? null : TestPassInputField.GetValue();
		}

		public void SetActiveSeparator(bool value)
		{
			if (SeparatorLayout)
				SeparatorLayout.gameObject.SetActive(value);
		}

		public void SetHeaderText(string value)
		{
			if (HeaderText)
				HeaderText.SetText(value);
		}

		public void SetAuthText(string value)
		{
			if (AuthText)
				AuthText.SetText(value);
		}

		public void SetEmailText(string value)
		{
			if (EmailInputField)
				EmailInputField.SetText(value);
		}

		public void SetPasswordText(string value)
		{
			if (PassInputField)
				PassInputField.SetText(value);
		}

		public void SetTestPasswordText(string value)
		{
			if (TestPassInputField)
				TestPassInputField.SetText(value);
		}

		public void SetSignInButtonText(string value)
		{
			if (SignInButton)
				SignInButton.SetText(value);
		}

		public void SetSignUpButtonText(string value)
		{
			if (SignUpButton)
				SignUpButton.SetText(value);
		}

		public void SetDeleteAccountButtonText(string value)
		{
			if (DeleteAccountButton)
				DeleteAccountButton.SetText(value);
		}

		public void SetGuestButtonText(string value)
		{
			if (GuestButton)
				GuestButton.SetText(value);
		}

		public void SetForgotPassButtonText(string value)
		{
			if (ForgotPassButton)
				ForgotPassButton.SetText(value);
		}

		public void SetActiveTestPassword(bool value)
		{
			if (TestPassInputField)
				TestPassInputField.gameObject.SetActive(value);
		}

		public void SetSignInAction(Action value)
		{
			if (SignInButton)
				SignInButton.Subscribe(value);
		}

		public void SetSignUpAction(Action value)
		{
			if (SignUpButton)
				SignUpButton.Subscribe(value);
		}

		public void SetDeleteAccountAction(Action value)
		{
			if (DeleteAccountButton)
				DeleteAccountButton.Subscribe(value);
		}

		public void SetForgotPassAction(Action value)
		{
			if (ForgotPassButton)
				ForgotPassButton.Subscribe(value);
		}

		public void SetGuestAction(Action value)
		{
			if (GuestButton)
				GuestButton.Subscribe(value);
		}

		private void Clear()
		{
			if (EmailInputField)
				EmailInputField.Clear();

			if (PassInputField)
				PassInputField.Clear();

			if (TestPassInputField)
				TestPassInputField.Clear();

			if (SignInButton)
				SignInButton.Clear();

			if (SignUpButton)
				SignUpButton.Clear();

			if (DeleteAccountButton)
				DeleteAccountButton.Clear();

			if (ForgotPassButton)
				ForgotPassButton.Clear();

			if (GuestButton)
				GuestButton.Clear();

			if (SocialWidget)
				SocialWidget.Clear();
		}
	}
}