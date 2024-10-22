using System;

namespace BerserkV3.Lobby.UI.Home.CommonWidgets.Abstractions
{
	public interface ICurrencyPanelPresenter
	{
		event Action OnWalletClicked;
		void Show();
		void Hide();
		void Init();
	}
}