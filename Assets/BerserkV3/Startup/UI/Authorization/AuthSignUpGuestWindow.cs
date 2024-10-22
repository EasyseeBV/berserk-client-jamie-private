using System;
using BerserkV3.Common.UIKit;
using BerserkV3.Common.UIService;
using RR.UIService.FullFade;
using TMPro;
using UnityEngine;

namespace BerserkV3.Startup.UI
{
	public class AuthSignUpGuestWindow : UISafeWindowBase, IFullFadeTarget
	{
		[SerializeField] protected TextMeshProUGUI HeaderText;
		[SerializeField] protected ComplexButton CancelButton;
		[SerializeField] protected SocialWidget SocialLayout;
		[SerializeField] protected CanvasGroup SeparateLayout;
		[SerializeField] protected ComplexInputField UserNameInputField;
		[SerializeField] protected ComplexInputField EmailInputField;
		[SerializeField] protected ComplexInputField PassInputField;
		[SerializeField] protected ComplexButton ContinueButton;
		[SerializeField] protected ComplexButton GuestButton;
		[SerializeField] protected ComplexButton LoginButton;

		protected override int InteractableDelayMs => 1000;
		public SocialWidget SocialWidget => SocialLayout;

		public Color? FadeColor => null;
		public void OnFadeClick() {}

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

		public string GetUserName()
		{
			return !UserNameInputField ? null : UserNameInputField.GetValue();
		}

		public string GetEmail()
		{
			return !EmailInputField ? null : EmailInputField.GetValue();
		}

		public string GetPassword()
		{
			return !PassInputField ? null : PassInputField.GetValue();
		}

		public void SetActiveSeparator(bool value)
		{
			if (SeparateLayout)
				SeparateLayout.gameObject.SetActive(value);
		}

		public void SetEmailInputText(string value)
		{
			if (EmailInputField)
				EmailInputField.SetText(value);
		}

		public void SetUserNameInputText(string value)
		{
			if (UserNameInputField)
				UserNameInputField.SetText(value);
		}

		public void SetPasswordInputText(string value)
		{
			if (PassInputField)
				PassInputField.SetText(value);
		}

		public void SetHeaderText(string value)
		{
			if (HeaderText)
				HeaderText.SetText(value);
		}

		public void SetLoginButtonText(string value)
		{
			if (LoginButton)
				LoginButton.SetText(value);
		}

		public void SetGuestButtonText(string value)
		{
			if (GuestButton)
				GuestButton.SetText(value);
		}

		public void SetContinueButtonText(string value)
		{
			if (ContinueButton)
				ContinueButton.SetText(value);
		}

		public void SetContinueAction(Action value)
		{
			if (ContinueButton)
				ContinueButton.Subscribe(value);
		}

		public void SetGuestAction(Action value)
		{
			if (GuestButton)
				GuestButton.Subscribe(value);
		}

		public void SetCancelAction(Action value)
		{
			if (CancelButton)
				CancelButton.Subscribe(value);
		}

		public void SetLoginAction(Action value)
		{
			if (LoginButton)
				LoginButton.Subscribe(value);
		}

		private void Clear()
		{
			if (SocialWidget)
				SocialWidget.Clear();

			if (ContinueButton)
				ContinueButton.Clear();

			if (GuestButton)
				GuestButton.Clear();

			if (LoginButton)
				LoginButton.Clear();

			if (UserNameInputField)
				UserNameInputField.Clear();

			if (EmailInputField)
				EmailInputField.Clear();

			if (PassInputField)
				PassInputField.Clear();
		}
	}
}