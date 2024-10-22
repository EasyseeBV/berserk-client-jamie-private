using System.Collections.Generic;
using BerserkV3.Common.UIKit;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Home.CommonWidgets
{
	public class CurrencyPanel : UIViewBase
	{
		[SerializeField] private List<CurrencyWidget> widgets;

		public List<CurrencyWidget> Widgets => widgets;

		public void Clear()
		{
			foreach (var widget in widgets)
			{
				widget?.Clear();
			}
		}
	}
}