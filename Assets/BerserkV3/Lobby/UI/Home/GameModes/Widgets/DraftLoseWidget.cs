using System.Collections.Generic;
using BerserkV3.Common.UIKit;
using TMPro;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Home.GameModes.Widgets
{
	public class DraftLoseWidget : UIViewBase
	{
		[SerializeField] private List<GameObject> regularWidgets = new();
		[SerializeField] private List<GameObject> loseWidgets = new();
		[SerializeField] private List<GameObject> gemWidgets = new();
		[SerializeField] private TextMeshProUGUI infoText;

		public void SetLoseCount(int value)
		{
			for (var i = 0; i < gemWidgets.Count; i++)
			{
				regularWidgets[i].SetActive(value <= 0);
				loseWidgets[i].SetActive(value > 0);
				gemWidgets[i].SetActive(value > 0);
				value--;
			}
		}

		public void SetInfoText(string value)
		{
			if (infoText)
				infoText.SetText(value);
		}
	}
}