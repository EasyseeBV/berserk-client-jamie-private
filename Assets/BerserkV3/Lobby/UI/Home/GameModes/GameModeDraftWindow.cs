using System;
using BerserkV3.Lobby.UI.Home.CommonWidgets;
using BerserkV3.Lobby.UI.Home.GameModes.Widgets;
using RR.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Lobby.UI.Home.GameModes
{
	public class GameModeDraftWindow : UIWindowBase
	{
		[SerializeField] private DraftLoseWidget loseWidget;
		[SerializeField] private DraftRewardsWidget rewardsWidget;
		[SerializeField] private SlotTimerUIWidget slotTimerWidget;
		[SerializeField] private TextMeshProUGUI playButtonText;
		[SerializeField] private Button playButton;
		[SerializeField] private Button joinButton;

		public DraftLoseWidget LoseWidget => loseWidget;
		public DraftRewardsWidget RewardsWidget => rewardsWidget;
		public SlotTimerUIWidget SlotTimerWidget => slotTimerWidget;

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

		public void SetPlayAction(Action value)
		{
			if (playButton)
				playButton.onClick.AddListener(() => value?.Invoke());
		}

		public void SetPlayButtonText(string value)
		{
			if (playButtonText)
				playButtonText.SetText(value);
		}

		public void SetJoinAction(Action value)
		{
			if (joinButton)
				joinButton.onClick.AddListener(() => value?.Invoke());
		}

		public void SetPlayButtonVisible(bool value)
		{
			if (playButton)
				playButton.gameObject.SetActive(value);
		}

		public void SetJoinButtonVisible(bool value)
		{
			if (joinButton)
				joinButton.gameObject.SetActive(value);
		}

		public void Clear()
		{
			if (joinButton)
				joinButton.onClick.RemoveAllListeners();

			if (playButton)
				playButton.onClick.RemoveAllListeners();

			if (loseWidget)
				loseWidget.SetLoseCount(0);

			if (rewardsWidget)
				rewardsWidget.Clear();
		}
	}
}