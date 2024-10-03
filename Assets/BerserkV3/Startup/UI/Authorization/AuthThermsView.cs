using System;
using BerserkV3.Common.Utils;

namespace BerserkV3.Startup.UI
{
	public partial class AuthThermsView : SafeView
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
			if (!string.IsNullOrEmpty(value))
				Set(MessageText, value);
		}
		
		public void SetButtonText(string value)
		{
			AcceptButton.SetText(value);
		}
		
		public void SetButtonInteractable(bool value)
		{
			AcceptButton.SetInteractable(value);
		}
		
		public void SetFooterText(string value)
		{
			FooterButton.SetText(value);
		}
		
		public void SetToggle(bool value)
		{
			AcceptToggle.isOn = value;
		}
		
		public void SetSubmitAction(Action value)
		{
			AcceptButton.Subscribe(value);
		}
		
		public void SetFooterAction(Action value)
		{
			FooterButton.Subscribe(value);
		}
		
		public void SetToggleChangeAction(Action<bool> value)
		{
			AcceptToggle.onValueChanged.AddListener(isOn => value?.Invoke(isOn));
		}
		
		private void Clear()
		{
			ScrollRect.ScrollToTop();
			AcceptButton.Clear();
			FooterButton.Clear();
			AcceptToggle.onValueChanged.RemoveAllListeners();
		}
	}
}