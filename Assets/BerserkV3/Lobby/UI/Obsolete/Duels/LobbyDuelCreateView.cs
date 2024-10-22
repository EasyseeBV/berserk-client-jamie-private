using System;
using BerserkV3.Lobby.MatchMaking.Duels;
using BerserkV3.Startup.UI;
using TMPro;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Duels
{
	public partial class LobbyDuelCreateView : SafeView
	{
		[SerializeField] protected TMP_InputField NameInput;
		[SerializeField] protected TMP_InputField PasswordInput;
		
		public string Name => NameInput.text;
		public string Password => PasswordInput.text;
		public bool IsPrivate => PrivateToggle.isOn;

		public void SetCreateAction(Action value)
		{
			OKButton.onClick.RemoveAllListeners();
			OKButton.onClick.AddListener(() => value?.Invoke());
		}

		public void SetCancelAction(Action value)
		{
			CancelButton.onClick.RemoveAllListeners();
			CancelButton.onClick.AddListener(() => value?.Invoke());
		}

		public void Clear()
		{
			if (OKButton)
				OKButton.onClick.RemoveAllListeners();
			
			if (CancelButton)
				CancelButton.onClick.RemoveAllListeners();
			
			if (PasswordInput)
				PasswordInput.onValueChanged.RemoveListener(ValidateInput);
			
			NameInput.text = string.Empty;
			PasswordInput.text = string.Empty;
			PrivateToggle.isOn = false;
		}
		
		private void ValidateInput(string value)
		{
			var changed = value.RemoveWhitespace();
			if (changed != value)
				PasswordInput.SetTextWithoutNotify(changed);
		}
		
		private void OnDestroy()
		{
			Clear();
		}
		
		protected override void OnShown()
		{
			base.OnShown();
			PasswordInput.onValueChanged.AddListener(ValidateInput);
		}

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
	}
}