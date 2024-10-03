using System;
using BerserkV3.Common.UIKit;

namespace BerserkV3.Startup.UI
{
	public partial class AuthSaveView : SafeView
	{
		public TimerWidget TimerWidget => SkipTimerWidget;
		protected override void OnHidden()
		{
			base.OnHidden();
			Clear();
		}

		protected override void OnClosed()
		{
			base.OnClosed();
			Clear();
		}

		private void OnDestroy()
		{
			Clear();
		}

		public void SetHeaderText(string value)
		{
			Set(HeaderText, value);
		}

		public void SetMessageText(string value)
		{
			Set(MessageText, value);
		}

		public void SetAcceptButtonText(string value)
		{
			AcceptButton.SetText(value);
		}

		public void SetCancelButtonText(string value)
		{
			CancelButton.SetText(value);
		}

		public void SetCancelAction(Action value)
		{
			CancelButton.Subscribe(value);
		}

		public void SetAcceptAction(Action value)
		{
			AcceptButton.Subscribe(value);
		}

		public void Clear()
		{
			CancelButton.Clear();
			AcceptButton.Clear();
			TimerWidget.Clear();
		}
	}
}