using BerserkV3.Common.UIKit;
using RR.UIService;
using UnityEngine;

namespace BerserkV3.Lobby.UI.Home.Collections
{
	public class CollectionsCardsWindow : UIWindowBase
	{
		[SerializeField] private ExtendedToggleGroup switcherView;

		public ISwitcherView SwitcherView => switcherView;
	}
}