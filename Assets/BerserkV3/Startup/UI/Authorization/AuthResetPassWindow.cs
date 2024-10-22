using System;
using BerserkV3.Common.UIKit;
using BerserkV3.Common.UIService;
using TMPro;
using UnityEngine;

namespace BerserkV3.Startup.UI
{
	public class AuthResetPassWindow : UISafeWindowBase
	{
		[SerializeField] protected TextMeshProUGUI HeaderText = default;
		[SerializeField] protected TextMeshProUGUI MessageText = default;
		[SerializeField] protected ComplexInputField CodeInputField = default;
		[SerializeField] protected ComplexInputField PassInputField = default;
		[SerializeField] protected ComplexButton ResetButton = default;
		[SerializeField] protected ComplexButton FooterButton = default;
		[SerializeField] protected ComplexButton ReturnButton = default;

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

		public string GetCodeInputText()
		{
			return !CodeInputField ? null : CodeInputField.GetValue();
		}

		public string GetPassInputText()
		{
			return !PassInputField ? null : PassInputField.GetValue();
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

		public void SetCodeInputText(string value)
		{
			if (CodeInputField)
				CodeInputField.SetText(value);
		}

		public void SetPassInputText(string value)
		{
			if (PassInputField)
				PassInputField.SetText(value);
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

		public void SetCodeInputValueChangeAction(Action<string> value)
		{
			if (CodeInputField)
				CodeInputField.SetValueChangeAction(value);
		}

		public void SetPassInputValueChangeAction(Action<string> value)
		{
			if (PassInputField)
				PassInputField.SetValueChangeAction(value);
		}

		private void Clear()
		{
			if (CodeInputField)
				CodeInputField.Clear();

			if (PassInputField)
				PassInputField.Clear();

			if (FooterButton)
				FooterButton.Clear();

			if (ResetButton)
				ResetButton.Clear();

			if (ReturnButton)
				ReturnButton.Clear();
		}
	}
}