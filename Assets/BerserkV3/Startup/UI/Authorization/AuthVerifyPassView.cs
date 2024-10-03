using System;

namespace BerserkV3.Startup.UI
{
	public partial class AuthVerifyPassView : SafeView
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

		public void SetHeaderText(string value)
		{
			Set(HeaderText, value);
		}
		
		public void SetMessageText(string value)
		{
			Set(MessageText, value);
		}
		
		public void SetFooterText(string value)
		{
			FooterButton.SetText(value);
		}
		
		public void SetFooterInteractable(bool value)
		{
			FooterButton.SetInteractable(value);
		}
		
		public void SetFooterAction(Action value)
		{
			FooterButton.Subscribe(value);
		}
		
		public void SetReturnAction(Action value)
		{
			ReturnButton.Subscribe(value);
		}
		
		private void Clear()
		{
			FooterButton.Clear();
			ReturnButton.Clear();
		}
	}
}