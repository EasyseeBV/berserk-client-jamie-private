using System;
using BerserkV3.Common.UIKit;
using BerserkV3.Lobby.UI.Home.CommonWidgets;
using RR.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Lobby.UI.Home.GameModes
{
	public class GameModeLeagueWindow : UIWindowBase
	{
		[SerializeField] private ExtendedToggleGroup leaguesGroup;
		[SerializeField] private ExtendedToggleGroup decksGroup;
		[SerializeField] private HeaderWidget leaguesHeaderWidget;
		[SerializeField] private HeaderWidget decksHeaderWidget;
		[SerializeField] private TextMeshProUGUI tooltipHeaderText;
		[SerializeField] private TextMeshProUGUI tooltipText;
		[SerializeField] private TextMeshProUGUI buttonText;
		[SerializeField] private Button playbutton;

		public ISwitcherView LeaguesSwitcher => leaguesGroup;
		public ISwitcherView DecksSwitcher => decksGroup;

		public HeaderWidget LeaguesHeader => leaguesHeaderWidget;
		public HeaderWidget DecksHeader => decksHeaderWidget;

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
			if (playbutton)
				playbutton.onClick.AddListener(() => value?.Invoke());
		}

		public void SetPlayInteractable(bool value)
		{
			if (playbutton)
				playbutton.interactable = value;
		}

		public void SetPlayButtonText(string value)
		{
			if(buttonText)
				buttonText.SetText(value);
		}

		public void SetTooltipHeaderText(string value)
		{
			if(tooltipHeaderText)
				tooltipHeaderText.SetText(value);
		}

		public void SetTooltipText(string value)
		{
			if(tooltipText)
				tooltipText.SetText(value);
		}

		public void Clear()
		{
			if (playbutton)
				playbutton.onClick.RemoveAllListeners();
			
			if(leaguesGroup)
				leaguesGroup.Release();
			
			if(decksGroup)
				decksGroup.Release();
		}
	}
}