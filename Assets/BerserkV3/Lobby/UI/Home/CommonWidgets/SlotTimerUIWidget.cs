using BerserkV3.Common.UIKit;
using BerserkV3.Lobby.UI.Home.CommonWidgets.Abstractions;
using TMPro;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Home.CommonWidgets
{
	public class SlotTimerUIWidget : UIViewBase, IUIWidget
	{
		[SerializeField] private TMP_Text[] digitLabels;

		public void UpdateTimerLabel(string timeString)
		{
			for (var i = 0; i < digitLabels.Length && i < timeString.Length; i++)
			{
				var number = timeString[i].ToString();
				if (digitLabels[i].text != number)
					digitLabels[i].text = number;
			}
		}
	}
}

