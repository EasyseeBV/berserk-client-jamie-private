using System;
using BerserkV3.Common.UIKit;
using BerserkV3.Common.UIService;
using TMPro;
using UnityEngine;

namespace BerserkV3.Startup.UI
{
	public class AuthForgotPassWindow : UISafeWindowBase
	{
		[SerializeField] protected TextMeshProUGUI HeaderText;
		[SerializeField] protected TextMeshProUGUI FieldText;
		[SerializeField] protected ComplexInputField InputField;
		[SerializeField] protected ComplexButton ResetButton;
		[SerializeField] protected ComplexButton FooterButton;
		[SerializeField] protected ComplexButton ReturnButton;

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

		public string GetInputText()
		{
			return !InputField ? null : InputField.GetValue();
		}

		public void SetHeaderText(string value)
		{
			if (HeaderText)
				HeaderText.SetText(value);
		}

		public void SetFieldText(string value)
		{
			if (FieldText)
				FieldText.SetText(value);
		}

		public void SetResetText(string value)
		{
			if (ResetButton)
				ResetButton.SetText(value);
		}

		public void SetResetInteractable(bool value)
		{
			if (ResetButton)
				ResetButton.SetInteractable(value);
		}

		public void SetFooterText(string value)
		{
			if (FooterButton)
				FooterButton.SetText(value);
		}

		public void SetInputText(string value)
		{
			if (InputField)
				InputField.SetText(value);
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

		public void SetResetAction(Action value)
		{
			if (ResetButton)
				ResetButton.Subscribe(value);
		}

		public void SetInputValueChangeAction(Action<string> value)
		{
			if (InputField)
				InputField.SetValueChangeAction(value);
		}

		private void Clear()
		{
			if (InputField)
				InputField.Clear();

			if (FooterButton)
				FooterButton.Clear();

			if (ResetButton)
				ResetButton.Clear();

			if (ReturnButton)
				ReturnButton.Clear();
		}
	}
}