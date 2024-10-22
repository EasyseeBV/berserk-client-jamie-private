using System;
using BerserkV3.Common.UIKit;
using BerserkV3.Common.UIService;
using UnityEngine;

namespace BerserkV3.Startup.UI
{
	public class AuthChooseWindow : UISafeWindowBase
	{
		[SerializeField] protected ComplexButton GuestButton;
		[SerializeField] protected ComplexButton SignInButton;
		[SerializeField] protected ComplexButton SignUpButton;
		
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