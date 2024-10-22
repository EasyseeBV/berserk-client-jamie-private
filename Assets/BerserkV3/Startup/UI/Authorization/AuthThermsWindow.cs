using System;
using BerserkV3.Common.UIKit;
using BerserkV3.Common.UIService;
using BerserkV3.Common.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Startup.UI
{
	public class AuthThermsWindow : UISafeWindowBase
	{
		[SerializeField] protected TextMeshProUGUI HeaderText;
		[SerializeField] protected ComplexButton AcceptButton;
		[SerializeField] protected ComplexButton FooterButton;
		[SerializeField] protected Toggle AcceptToggle;
		[SerializeField] protected ScrollRect ScrollRect;
		[SerializeField] protected TextMeshProUGUI MessageText;

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
			if (!string.IsNullOrEmpty(value) && MessageText)
				MessageText.SetText(value);
		}

		public void SetButtonText(string value)
		{
			if (AcceptButton)
				AcceptButton.SetText(value);
		}

		public void SetButtonInteractable(bool value)
		{
			if (AcceptButton)
				AcceptButton.SetInteractable(value);
		}

		public void SetFooterText(string value)
		{
			if (FooterButton)
				FooterButton.SetText(value);
		}

		public void SetToggle(bool value)
		{
			if (AcceptToggle)
				AcceptToggle.isOn = value;
		}

		public void SetSubmitAction(Action value)
		{
			if (AcceptButton)
				AcceptButton.Subscribe(value);
		}

		public void SetFooterAction(Action value)
		{
			if (FooterButton)
				FooterButton.Subscribe(value);
		}

		public void SetToggleChangeAction(Action<bool> value)
		{
			if (AcceptToggle)
				AcceptToggle.onValueChanged.AddListener(isOn => value?.Invoke(isOn));
		}

		private void Clear()
		{
			if (ScrollRect)
				ScrollRect.ScrollToTop();

			if (AcceptButton)
				AcceptButton.Clear();

			if (FooterButton)
				FooterButton.Clear();

			if (AcceptToggle)
				AcceptToggle.onValueChanged.RemoveAllListeners();
		}
	}
}