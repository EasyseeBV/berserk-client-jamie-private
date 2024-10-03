using System;

namespace BerserkV3.Startup.UI
{
	public partial class AuthMessageView : SafeView
	{
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

		public void SetSuccess(bool value)
		{
			SetActive(SuccessImage, value);
			SetActive(FailImage, !value);
		}
		
		public void SetHeaderText(string value)
		{
			Set(HeaderText, value);
		}
		
		public void SetMessageText(string value)
		{
			Set(MessageText, value);
		}
		
		public void SetButtonText(string value)
		{
			AcceptButton.SetText(value);
		}

		public void SetOnSubmit(Action value)
		{
			AcceptButton.Subscribe(value);
		}

		private void Clear()
		{
			AcceptButton.Clear();
		}
	}
}