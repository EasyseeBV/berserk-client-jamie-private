using System;
using BerserkV3.Common.UIKit;
using BerserkV3.Common.UIService;
using TMPro;
using UnityEngine;

namespace BerserkV3.Startup.UI
{
	public class AuthVerifyPassWindow : UISafeWindowBase
	{
		[SerializeField] protected TextMeshProUGUI HeaderText;
		[SerializeField] protected TextMeshProUGUI MessageText;
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

			if (ReturnButton)
				ReturnButton.Clear();
		}
	}
}