using BerserkV3.Common.UIKit;
using TMPro;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Home.GameModes.Widgets
{
	public class DraftRewardGemWidget : UIViewBase
	{
		[SerializeField] private TextMeshProUGUI gemLabel;

		public void SetText(string value)
		{
			if (gemLabel)
				gemLabel.SetText(value);
		}
	}
}