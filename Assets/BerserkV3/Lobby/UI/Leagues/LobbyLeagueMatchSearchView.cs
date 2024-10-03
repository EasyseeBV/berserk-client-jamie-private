using System;
using RR.UI.FrameSystem;

namespace BerserkV3.Lobby.UI.Leagues
{
	public interface ILobbyLeagueMatchSearchView
	{
		bool IsVisible { get; }
		void SetCancelAction(Action value);

		void SetAcceptAction(Action<bool> value);

		void SetCancelInteraction(bool value);

		void SetTimerText(string value);

		void SetTitleText(string value);
		
		void SetAcceptButtonVisible(bool value);

		void SetDeclineButtonVisible(bool value);

		void SetCancelButtonVisible(bool value);

		void Clear();
		void Show();
		void Close();

	}
	public partial class LobbyLeagueMatchSearchView : BaseView, ILobbyLeagueMatchSearchView
	{
		public bool IsVisible => VisibleState == VisibleState.Visible;

		public void SetCancelAction(Action value)
		{
			CancelBtn.Subscribe(() => value?.Invoke());
		}

		public void SetAcceptAction(Action<bool> value)
		{
			DeclineBtn.Subscribe(() => value?.Invoke(false));
			AcceptBtn.Subscribe(() => value?.Invoke(true));
		}

		public void SetCancelInteraction(bool value)
		{
			CancelBtn.SetInteractable(value);
		}

		public void SetTimerText(string value)
		{
			TimerText.SetText(value);
		}

		public void SetTitleText(string value)
		{
			TitleText.SetText(value);
		}

		public void SetAcceptButtonVisible(bool value)
		{
			AcceptBtn.gameObject.SetActive(value);
			AcceptBtn.SetInteractable(value);
		}

		public void SetDeclineButtonVisible(bool value)
		{
			DeclineBtn.gameObject.SetActive(value);
			AcceptBtn.SetInteractable(value);
		}

		public void SetCancelButtonVisible(bool value)
		{
			CancelBtn.gameObject.SetActive(value);
		}

		public void Clear()
		{
			if (CancelBtn)
				CancelBtn.UnSubscribeAll();
			
			if (AcceptBtn)
				AcceptBtn.UnSubscribeAll();
			
			if (DeclineBtn)
				DeclineBtn.UnSubscribeAll();
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
	}
}