using System;
using BerserkV3.Common.UIKit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Lobby.UI.Home.General.Widgets
{
	public class SearchingMatchWidget : UIViewBase
	{
		[SerializeField] private TextMeshProUGUI searchingText;
		[SerializeField] private TextMeshProUGUI timerText;
		[SerializeField] private Button cancelButton;
		
		public void SetText(string value)
		{
			if(searchingText)
				searchingText.SetText(value);
		}
		
		public void SetTimerText(string value)
		{
			if(timerText)
				timerText.SetText(value);
		}
		
		public void SetCancelAction(Action value)
		{
			if(cancelButton)
				cancelButton.onClick.AddListener(() => value?.Invoke());
		}

		public void SetActiveTimer(bool value)
		{
			if (timerText)
				timerText.gameObject.SetActive(value);
		}

		public void Clear()
		{
			if(cancelButton)
				cancelButton.onClick.RemoveAllListeners();
			
			SetActiveTimer(false);
		}
	}
}