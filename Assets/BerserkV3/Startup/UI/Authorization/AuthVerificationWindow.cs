using System;
using BerserkV3.Common.UIKit;
using BerserkV3.Common.UIService;
using TMPro;
using UnityEngine;

namespace BerserkV3.Startup.UI
{
	public class AuthVerificationWindow : UISafeWindowBase
	{
		[SerializeField] protected TextMeshProUGUI HeaderText;
		[SerializeField] protected TextMeshProUGUI MessageText;
		[SerializeField] protected ComplexButton SubmitButton;
		[SerializeField] protected ComplexInputField InputField;
		[SerializeField] protected ComplexButton ReturnButton;
		[SerializeField] protected ComplexButton FooterButton;
		[SerializeField] protected TimerWidget timerWidget;

		public TimerWidget TimerWidget => timerWidget;

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

		public string GetInput()
		{
			return !InputField ? null : InputField.GetValue();
		}

		public void SetInputText(string value)
		{
			if (InputField)
				InputField.SetText(value);
		}

		public void SetHeaderText(string value)
		{
			if (HeaderText)
				HeaderText.SetText(value);
		}

		public void SetMessageText(string value)
		{
			if (MessageText)
				MessageText.SetText(value);
		}

		public void SetSubmitText(string value)
		{
			if (SubmitButton)
				SubmitButton.SetText(value);
		}

		public void SetFooterText(string value)
		{
			if (FooterButton)
				FooterButton.SetText(value);
		}

		public void SetFooterInteractable(bool value)
		{
			if (FooterButton)
				FooterButton.SetInteractable(value);
		}

		public void SetSubmitInteractable(bool value)
		{
			if (SubmitButton)
				SubmitButton.SetInteractable(value);
		}

		public void SetInputChangedAction(Action<string> value)
		{
			if (InputField)
				InputField.SetValueChangeAction(value);
		}

		public void SetSubminAction(Action value)
		{
			if (SubmitButton)
				SubmitButton.Subscribe(value);
		}

		public void SetFooterAction(Action value)
		{
			if (FooterButton)
				FooterButton.Subscribe(value);
		}

		public void SetReturnAction(Action value)
		{
			if (ReturnButton)
				ReturnButton.Subscribe(value);
		}

		private void Clear()
		{
			if (FooterButton)
				FooterButton.Clear();

			if (SubmitButton)
				SubmitButton.Clear();

			if (ReturnButton)
				ReturnButton.Clear();

			if (InputField)
				InputField.Clear();
		}
	}
}