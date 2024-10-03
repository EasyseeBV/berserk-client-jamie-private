using System.Linq;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Common.UIKit
{
	public class HorizontalHexagoneLayout : BaseView
	{
		[SerializeField] private RectTransform contentContainer;
		[SerializeField, Min(1)] private int maxItemsRowA = 1;
		[SerializeField, Min(1)] private int maxItemsRowB = 1;
		[SerializeField] private float horizontalSpace;
		[SerializeField] private float verticalSpace;
		[SerializeField] private bool horizontalFitter;
		[SerializeField] private bool verticalFitter;
		[SerializeField] private bool gapOnFirstRow;
		[SerializeField] private bool includeDisabled;
		[SerializeField] private bool useScale;
		[SerializeField] private float PaddingLeft;
		[SerializeField] private float PaddingRight;
		[SerializeField] private float PaddingTop;
		[SerializeField] private float PaddingBottom;
			

		[ContextMenu("RefreshLayout")]
		public void RefreshLayout()
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(contentContainer);
			var rowIndex = 0;
			var itemRowIndex = 0;
			var items = contentContainer
				.GetChildren()
				.OfType<RectTransform>()
				.Where(x => includeDisabled || x.gameObject.activeSelf)
				.ToArray();
			var itemsCount = items.Length;
			var itemSize = Vector2.zero;
			if (itemsCount > 0)
			{
				itemSize = items[0].sizeDelta;
				itemSize = useScale ? itemSize * items[0].localScale : itemSize;
			}
			var gapSize = itemSize / 2f;
			var hasGap = gapOnFirstRow;
			var bounds = Vector2.zero;
			for (var i = 0; i < itemsCount; i++)
			{
				var currentRowCount = rowIndex % 2 == 0 ? maxItemsRowA : maxItemsRowB;
				var item = items[i];

				var itemPos = gapSize;
				itemPos.y = -gapSize.y;
				itemPos.y -= rowIndex * verticalSpace;
				itemPos.y -= rowIndex * itemSize.y;
				itemPos.y -= PaddingTop;
				itemPos.x += itemRowIndex * horizontalSpace;
				itemPos.x += itemRowIndex * itemSize.x;
				itemPos.x += PaddingLeft;
				itemPos.x += hasGap && itemsCount > maxItemsRowA ? gapSize.x + (horizontalSpace / 2f) : 0;
				item.anchoredPosition = itemPos;
				itemRowIndex++;

				if (Mathf.Abs(itemPos.x) > Mathf.Abs(bounds.x))
					bounds.x = Mathf.Abs(itemPos.x + PaddingRight);

				if (Mathf.Abs(itemPos.y) > Mathf.Abs(bounds.y))
					bounds.y = Mathf.Abs(itemPos.y + -PaddingBottom);

				if (itemRowIndex < currentRowCount || i + 1 >= itemsCount)
					continue;

				itemRowIndex = 0;
				hasGap = !hasGap;
				rowIndex++;
			}

			var fitSize = contentContainer.sizeDelta;
			if (horizontalFitter)
				fitSize.x = bounds.x + gapSize.x;

			if (verticalFitter)
				fitSize.y = bounds.y + gapSize.y;

			if (horizontalFitter || verticalFitter)
				contentContainer.sizeDelta = fitSize;
		}

		private void OnValidate()
		{
			if (contentContainer)
				RefreshLayout();
		}
	}
}