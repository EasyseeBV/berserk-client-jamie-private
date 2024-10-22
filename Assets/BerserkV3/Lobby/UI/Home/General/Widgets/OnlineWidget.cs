using BerserkV3.Common.UIKit;
using TMPro;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Home.General.Widgets
{
	public class OnlineWidget : UIViewBase
	{
		[SerializeField] private TextMeshProUGUI onlineText;
		[SerializeField] private TextMeshProUGUI countText;
		
		public void SetText(string value)
		{
			if(onlineText)
				onlineText.SetText(value);
		}
		
		public void SetCountText(string value)
		{
			if(countText)
				countText.SetText(value);
		}
	}
}