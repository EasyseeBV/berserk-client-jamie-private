using System;
using Berserk.Shared.Data.Shop;
using BerserkV3.Common.UIKit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Lobby.UI.Home.CommonWidgets
{
	public class CurrencyWidget : UIViewBase
	{
		[SerializeField] private Button button;
		[SerializeField] private TMP_Text currencyIcon;
		[SerializeField] private TMP_Text amountLabel;

		public WalletType WalletType { get; private set; }

		public void Init(WalletType walletType, Action clickAction)
		{
			WalletType = walletType;
			button.onClick.AddListener(() => clickAction?.Invoke());
		}
		
		public void SetIcon(string icon)
		{
			currencyIcon.text = icon;
		}

		public void SetAmount(int amount)
		{
			amountLabel.text = amount.ToString("N0");
		}

		public void Clear()
		{
			button.onClick.RemoveAllListeners();
		}
		
		private void OnDestroy()
		{
			Clear();
		}
	}
}