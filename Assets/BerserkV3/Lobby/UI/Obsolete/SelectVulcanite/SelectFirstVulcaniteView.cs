using System;
using BerserkV3.Common.UIKit;
using Global;
using RR.Core.Extensions;
using UnityEngine;

namespace BerserkV3.Lobby.UI
{
	public partial class SelectFirstVulcaniteView : View
	{
		[SerializeField] protected HexagoneItemView HexagoneItemPrefab;
		public HorizontalHexagoneLayout HexagoneLayout => ScrollHexagoneLayout;
		public HexagoneItemView HexagoneItemView => HexagoneItemPrefab;

		protected override void OnClosed()
		{
			Clear();
			base.OnClosed();
		}

		protected override void OnHidden()
		{
			Clear();
			base.OnHidden();
		}

		private void OnDestroy()
		{
			Clear();
		}

		public void SetHeaderText(string value)
		{
			if (HeaderText)
				HeaderText.SetText(value);
		}

		public void SetAcceptAction(Action value)
		{
			OKButton.Subscribe(() => value?.Invoke());
		}

		public void Clear()
		{
			ScrollHexagoneLayout.DestroyChildrenExcept(HexagoneItemPrefab.transform);
			OKButton.UnSubscribeAll();
		}
	}
}