using System;
using BerserkV3.Common.UIKit;
using BerserkV3.Common.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Startup.UI
{
	public class AuthMessageWindow : UISafeWindowBase
	{
		[SerializeField] protected Image FailImage;
		[SerializeField] protected Image SuccessImage;
		[SerializeField] protected TextMeshProUGUI HeaderText;
		[SerializeField] protected TextMeshProUGUI MessageText;
		[SerializeField] protected ComplexButton AcceptButton;

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

		public void SetSuccess(bool value)
		{
			if (SuccessImage)
				SuccessImage.gameObject.SetActive(value);

			if (FailImage)
				FailImage.gameObject.SetActive(!value);
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

		public void SetButtonText(string value)
		{
			if (AcceptButton)
				AcceptButton.SetText(value);
		}

		public void SetOnSubmit(Action value)
		{
			if (AcceptButton)
				AcceptButton.Subscribe(value);
		}

		private void Clear()
		{
			if (AcceptButton)
				AcceptButton.Clear();
		}
	}
}