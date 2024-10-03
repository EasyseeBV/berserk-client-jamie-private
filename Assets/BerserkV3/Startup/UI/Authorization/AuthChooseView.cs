using System;

namespace BerserkV3.Startup.UI
{
	public partial class AuthChooseView : SafeView
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
		
		public void SetGuestButtonText(string value)
		{
			GuestButton.SetText(value);
		}

		public void SetSignInButtonText(string value)
		{
			SignInButton.SetText(value);
		}

		public void SetSignUpButtonText(string value)
		{ 
			SignUpButton.SetText(value);
		}

		public void SetGuestAction(Action value)
		{
			GuestButton.Subscribe(value);
		}

		public void SetSignInAction(Action value)
		{
			SignInButton.Subscribe(value);
		}

		public void SetSignUpAction(Action value)
		{
			SignUpButton.Subscribe(value);
		}

		public void Clear()
		{
			GuestButton.Clear();
			SignInButton.Clear();
			SignUpButton.Clear();
		}
	}
}