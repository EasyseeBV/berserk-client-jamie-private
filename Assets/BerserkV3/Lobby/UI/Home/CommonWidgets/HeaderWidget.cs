using BerserkV3.Common.UIKit;
using TMPro;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Home.CommonWidgets
{
	public class HeaderWidget : UIViewBase
	{
		[SerializeField] private TMP_Text headerText;
		private bool releasePrevious;

		public void SetText(string value)
		{
			headerText.SetText(value);
		}
	}
}