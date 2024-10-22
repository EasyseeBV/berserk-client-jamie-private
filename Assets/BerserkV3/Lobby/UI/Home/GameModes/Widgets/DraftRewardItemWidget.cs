using System;
using System.Linq;
using System.Threading;
using Berserk.Shared.Data.Shop.Enums;
using BerserkV3.Common.UIKit;
using BerserkV3.Lobby.UI.Store.Widgets;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Lobby.UI.Home.GameModes.Widgets
{
	public class DraftRewardItemWidget : UIViewBase
	{
		[SerializeField] private Button button;
		[SerializeField] private ProductBorderMap productBorderMap = new();

		public UniTask InitAsync(string artUrl, BorderType border, CancellationToken token)
		{
			var frameWidget = productBorderMap.SetOneVisible(border);
			return frameWidget.InitAsync(artUrl, token);
		}

		public void SetClickAction(Action value)
		{
			if (button)
				button.onClick.AddListener(() => value?.Invoke());
		}

		public void SetInteractable(bool value)
		{
			if (button)
				button.interactable = value;
		}

		public void Clear()
		{
			if (button)
				button.onClick.RemoveAllListeners();
			
			productBorderMap.Values.Where(component => component).ForEach(widget => widget.Clear());
		}

		private void OnDestroy()
		{
			Clear();
		}
	}
}