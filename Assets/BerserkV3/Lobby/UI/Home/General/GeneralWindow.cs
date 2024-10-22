using BerserkV3.Common.UIKit;
using BerserkV3.Lobby.UI.Home.CommonWidgets;
using BerserkV3.Lobby.UI.Home.General.Widgets;
using UnityEngine;
using UIWindowBase = RR.UIService.UIWindowBase;

namespace BerserkV3.Lobby.UI.Home.General
{
	public class GeneralWindow : UIWindowBase
	{
		[SerializeField] private ExtendedToggleGroup switcherView;
		[SerializeField] private PlayerAvatarWidget profileWidget;
		[SerializeField] private SearchingMatchWidget searchingWidget;
		[SerializeField] private OnlineWidget onlineWidget;
		[SerializeField] private CurrencyPanel currencyPanel;

		public ISwitcherView SwitcherView => switcherView;
		public PlayerAvatarWidget ProfileWidget => profileWidget;
		public SearchingMatchWidget SearchingWidget => searchingWidget;
		public OnlineWidget OnlineWidget => onlineWidget;
		public CurrencyPanel CurrencyPanel => currencyPanel;
	}
}