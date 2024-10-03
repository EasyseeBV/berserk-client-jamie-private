using System;
using BerserkV3.Lobby.MatchMaking.Duels;
using BerserkV3.Startup.UI;

namespace BerserkV3.Lobby.UI.Duels
{
	public partial class LobbyDuelJoinView : SafeView
	{
		public string RoomCode => RoomCodeInput.text;
		public string Password => PasswordInput.text;

		public void SetActiveRoomCode(bool value)
		{
			SetActive(RoomCodeInput, value);
		}
		
		public void SetJoinAction(Action value)
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
			if (CancelButton)
				CancelButton.onClick.RemoveAllListeners();
			
			if (OKButton)
				OKButton.onClick.RemoveAllListeners();
			
			if (PasswordInput)
				PasswordInput.onValueChanged.RemoveListener(ValidateInput);
		}
		
		private void ValidateInput(string value)
		{
			var changed = value.RemoveWhitespace();
			if (changed != value)
				PasswordInput.SetTextWithoutNotify(changed);
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