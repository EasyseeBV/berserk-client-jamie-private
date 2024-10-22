using System;
using BerserkV3.Common.UIKit;
using BerserkV3.Common.UIService;
using TMPro;
using UnityEngine;

namespace BerserkV3.Startup.UI
{
	public class AuthReferralWindow : UISafeWindowBase
	{
		[SerializeField] protected TextMeshProUGUI HeaderText = default;
		[SerializeField] protected TextMeshProUGUI MessageText = default;
		[SerializeField] protected ComplexButton SkipButton = default;
		[SerializeField] protected ComplexButton SubmitButton = default;
		[SerializeField] protected ComplexInputField InputField = default;

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

		public void SetInputText(string value)
		{
			if (InputField)
				InputField.SetText(value);
		}

		public void SetSkipText(string value)
		{
			if (SkipButton)
				SkipButton.SetText(value);
		}

		public void SetSubmitText(string value)
		{
			if (SubmitButton)
				SubmitButton.SetText(value);
		}

		public string GetInputText()
		{
			return !InputField ? null : InputField.GetValue();
		}

		public void SetInputChangeAction(Action<string> action)
		{
			if (InputField)
				InputField.SetValueChangeAction(action);
		}

		public void SetSubmitAction(Action value)
		{
			if (SubmitButton)
				SubmitButton.Subscribe(value);
		}

		public void SetSkipAction(Action value)
		{
			if (SkipButton)
				SkipButton.Subscribe(value);
		}

		public void SetSubmitInteractable(bool value)
		{
			if (SubmitButton)
				SubmitButton.SetInteractable(value);
		}

		private void Clear()
		{
			if (SubmitButton)
				SubmitButton.Clear();

			if (SkipButton)
				SkipButton.Clear();

			if (InputField)
				InputField.Clear();
		}
	}
}