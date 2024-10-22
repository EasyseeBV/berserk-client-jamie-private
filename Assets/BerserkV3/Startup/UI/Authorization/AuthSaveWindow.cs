using System;
using BerserkV3.Common.UIKit;
using BerserkV3.Common.UIService;
using TMPro;
using UnityEngine;

namespace BerserkV3.Startup.UI
{
	public class AuthSaveWindow : UISafeWindowBase
	{
		[SerializeField] protected TextMeshProUGUI HeaderText;
		[SerializeField] protected TextMeshProUGUI MessageText;
		[SerializeField] protected TimerWidget SkipTimerWidget;
		[SerializeField] protected ComplexButton CancelButton;
		[SerializeField] protected ComplexButton AcceptButton;

		public TimerWidget TimerWidget => SkipTimerWidget;

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

		public void SetAcceptButtonText(string value)
		{
			if (AcceptButton)
				AcceptButton.SetText(value);
		}

		public void SetCancelButtonText(string value)
		{
			if (CancelButton)
				CancelButton.SetText(value);
		}

		public void SetCancelAction(Action value)
		{
			if (CancelButton)
				CancelButton.Subscribe(value);
		}

		public void SetAcceptAction(Action value)
		{
			if (AcceptButton)
				AcceptButton.Subscribe(value);
		}

		public void Clear()
		{
			if (CancelButton)
				CancelButton.Clear();

			if (AcceptButton)
				AcceptButton.Clear();

			if (TimerWidget)
				TimerWidget.Clear();
		}
	}
}