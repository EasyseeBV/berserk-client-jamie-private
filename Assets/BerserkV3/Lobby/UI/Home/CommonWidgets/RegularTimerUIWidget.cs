using BerserkV3.Common.UIKit;
using BerserkV3.Lobby.UI.Home.CommonWidgets.Abstractions;
using TMPro;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Home.CommonWidgets
{
	public class RegularTimerUIWidget : UIViewBase, IUIWidget
	{
		[SerializeField] private TMP_Text timerLabel;
		
		public void UpdateTimerLabel(string timeString)
		{
			if (timerLabel != null)
			{
				timerLabel.text = timeString;
			}
		}
	}
}