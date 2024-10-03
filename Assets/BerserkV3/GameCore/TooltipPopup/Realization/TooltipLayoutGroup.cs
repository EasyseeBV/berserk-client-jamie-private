using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.GameCore.TooltipPopup
{
	[ExecuteAlways]
	public class TooltipLayoutGroup : LayoutGroup
	{
		[SerializeField] private RectTransform canvas;
		[Space]
		[SerializeField] private GridLayoutGroup.Corner startCorner = GridLayoutGroup.Corner.UpperLeft;
		[SerializeField] protected float spacing = 0;
		[SerializeField] private RectOffset margins;
		
		private List<float> columnsWidthMin = new();
		private List<float> columnsWidthPreferred = new();
		private List<float> columnsWidthFlexible = new();

		private float screenHeight => canvas.rect.size[1] - margins.vertical;
		
		public float Spacing
		{
			get => spacing;
			set => SetProperty(ref spacing, value);
		}
		
		public RectOffset Margins
		{
			get => margins;
			set => SetProperty(ref margins, value);
		}

		public override void CalculateLayoutInputHorizontal()
		{
			base.CalculateLayoutInputHorizontal();
			CalcAlongHorizontal();
		}

		public override void CalculateLayoutInputVertical()
		{
			CalcAlongVertical();
		}

		public override void SetLayoutHorizontal()
		{
			SetChildrenAlongHorizontal();
		}

		public override void SetLayoutVertical()
		{
			SetChildrenAlongVertical();
		}

		//HINT:
		//axis:
		//0 - horizontal
		//1 - vertical

		private void CalcAlongHorizontal()
		{
			columnsWidthMin = new() { 0 };
			columnsWidthPreferred = new() { 0 };
			columnsWidthFlexible = new() { 0 };
			
			bool controlSize = true;
			bool useScale = true;
			bool childForceExpandSize = true;

			float combinedPadding = padding.horizontal;
			float totalMin = combinedPadding;
			float totalPreferred = combinedPadding;
			float totalFlexible = 0;
			float totalHeight = padding.top + padding.bottom;
			int column = 0;

			var rectChildrenCount = rectChildren.Count;
			
			for (int i = 0; i < rectChildrenCount; i++)
			{
				RectTransform child = rectChildren[i];
				GetChildSizes(child, 0, controlSize, childForceExpandSize, out var min, out var preferred, out var flexible);
				GetChildSizes(child, 1, controlSize, childForceExpandSize, out _, out var preferredChildHeight, out _);

				if (useScale)
				{
					var localScale = child.localScale;
					float scaleFactor = localScale[0];
					min *= scaleFactor;
					preferred *= scaleFactor;
					flexible *= scaleFactor;
					preferredChildHeight *= localScale[1];
				}

				totalHeight += preferredChildHeight;

				if (totalHeight > screenHeight)
				{
					totalHeight = padding.top + padding.bottom + preferredChildHeight;
					columnsWidthMin.Add(0);
					columnsWidthPreferred.Add(0);
					columnsWidthFlexible.Add(0);
					column += 1;
				}

				totalHeight += spacing;
				
				columnsWidthMin[column] = Mathf.Max(columnsWidthMin[column], min);
				columnsWidthPreferred[column] = Mathf.Max(columnsWidthPreferred[column], preferred);
				columnsWidthFlexible[column] = Mathf.Max(columnsWidthFlexible[column], flexible);
			}

			if (rectChildren.Count > 0)
			{
				totalMin += columnsWidthMin.Sum() + spacing * (columnsWidthMin.Count - 1);
				totalPreferred += columnsWidthPreferred.Sum() + spacing * (columnsWidthPreferred.Count - 1);
				totalFlexible += columnsWidthFlexible.Sum();
			}

			totalPreferred = Mathf.Max(totalMin, totalPreferred);
			SetLayoutInputForAxis(totalMin, totalPreferred, totalFlexible, 0);
		}

		private void SetChildrenAlongHorizontal()
		{
			bool controlSize = true;
			bool useScale = true;
			bool childForceExpandSize = true;
			
			float totalHeight = padding.top + padding.bottom;
			int column = 0;
			float size = rectTransform.rect.size[0];
			float pos = padding.left;
			float surplusSpace = size - GetTotalPreferredSize(0);
			
			var rectChildrenCount = rectChildren.Count;

			if (surplusSpace > 0 && GetTotalFlexibleSize(0) == 0)
				pos = GetStartOffset(0, GetTotalPreferredSize(0) - padding.horizontal);
			
			for (int i = 0; i < rectChildrenCount; i++)
			{
				RectTransform child = rectChildren[i];
				GetChildSizes(child, 1, controlSize, childForceExpandSize, out _, out var preferredChildHeight, out _);
				float scaleFactor = useScale ? child.localScale[0] : 1f;

				totalHeight += preferredChildHeight;
				
				float childSize = columnsWidthPreferred[column];

				if (totalHeight > screenHeight)
				{
					totalHeight = padding.top + padding.bottom + preferredChildHeight;
					pos += columnsWidthPreferred[column] + spacing;
					column += 1;
					childSize = columnsWidthPreferred[column];
				}

				totalHeight += spacing;
				
				var resultPos = pos;
				if (startCorner is GridLayoutGroup.Corner.UpperRight or GridLayoutGroup.Corner.LowerRight)
				{
					resultPos = rectTransform.rect.size[0] - pos - childSize;
				}
				
				SetChildAlongAxisWithScale(child, 0, resultPos, childSize, scaleFactor);
			}
		}

		private void SetChildrenAlongVertical()
		{
			bool controlSize = true;
			bool useScale = true;
			bool childForceExpandSize = true;
			
			float totalHeight = padding.top + padding.bottom;
			float size = rectTransform.rect.size[1];
			float pos = padding.top;
			float surplusSpace = size - GetTotalPreferredSize(1);
			
			var rectChildrenCount = rectChildren.Count;

			if (surplusSpace > 0 && GetTotalFlexibleSize(1) == 0)
				pos = GetStartOffset(1, GetTotalPreferredSize(1) - padding.vertical);
			
			for (int i = 0; i < rectChildrenCount; i++)
			{
				RectTransform child = rectChildren[i];
				GetChildSizes(child, 1, controlSize, childForceExpandSize, out _, out var preferred, out _);
				float scaleFactor = useScale ? child.localScale[0] : 1f;
				float childSize = preferred;

				totalHeight += preferred;

				if (totalHeight > screenHeight)
				{
					totalHeight = padding.top + padding.bottom + preferred;
					if (surplusSpace > 0 && GetTotalFlexibleSize(1) == 0)
						pos = GetStartOffset(1, GetTotalPreferredSize(1) - padding.vertical);
					else
						pos = padding.top;
				}

				totalHeight += spacing;

				var resultPos = pos;
				if (startCorner is GridLayoutGroup.Corner.LowerLeft or GridLayoutGroup.Corner.LowerRight)
				{
					resultPos = rectTransform.rect.size[1] - pos - childSize;
				}
				
				SetChildAlongAxisWithScale(child, 1, resultPos, childSize, scaleFactor);

				pos += childSize * scaleFactor + spacing;
			}
		}

		private void CalcAlongVertical()
		{
			bool useScale = true;
			
			float combinedPadding = padding.vertical;
			float totalMin = combinedPadding;
			float totalPreferred = combinedPadding;
			float totalFlexible = 0;

			var rectChildrenCount = rectChildren.Count;
			
			for (int i = 0; i < rectChildrenCount; i++)
			{
				RectTransform child = rectChildren[i];
				GetChildSizes(child, 1, false, false, out var min, out var preferred, out var flexible);

				if (useScale)
				{
					float scaleFactor = child.localScale[1];
					min *= scaleFactor;
					preferred *= scaleFactor;
					flexible *= scaleFactor;
				}

				totalMin += min;
				totalPreferred += preferred;
				totalFlexible += flexible;

				if (totalPreferred > screenHeight)
				{
					totalMin = screenHeight;
					totalPreferred = screenHeight;
					break;
				}

				if (i < rectChildrenCount - 1)
				{
					totalMin += spacing;
					totalPreferred += spacing;
				}
			}

			totalPreferred = Mathf.Max(totalMin, totalPreferred);
			SetLayoutInputForAxis(totalMin, totalPreferred, totalFlexible, 1);
		}
		
		private void GetChildSizes(RectTransform child, int axis, bool controlSize, bool childForceExpand,
			out float min, out float preferred, out float flexible)
		{
			if (!controlSize)
			{
				min = child.sizeDelta[axis];
				preferred = min;
				flexible = 0;
			}
			else
			{
				min = LayoutUtility.GetMinSize(child, axis);
				preferred = LayoutUtility.GetPreferredSize(child, axis);
				flexible = LayoutUtility.GetFlexibleSize(child, axis);
			}

			if (childForceExpand)
				flexible = Mathf.Max(flexible, 1);
		}
	}
}