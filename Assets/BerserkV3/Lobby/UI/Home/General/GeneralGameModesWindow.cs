using BerserkV3.Common.UIKit;
using BerserkV3.Lobby.UI.Home.CommonWidgets;
using RR.UIService;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Home.General
{
	public class GeneralGameModesWindow : UIWindowBase
	{
		[SerializeField] private ExtendedToggleGroup switcherView;
		[SerializeField] private HeaderWidget headerWidget;

		public ISwitcherView SwitcherView => switcherView;
		public HeaderWidget HeaderWidget => headerWidget;
	}
}