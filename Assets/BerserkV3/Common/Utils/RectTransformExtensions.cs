using UnityEngine;

namespace Berserk.Shared.GameCore.Utils
{
	public static class RectTransformExtensions
	{
		public static RectTransformData CopyRectTransformData(this RectTransform rectTransform)
		{
			return new RectTransformData
			{
				LocalPosition = rectTransform.localPosition,
				LocalScale = rectTransform.localScale,
				SizeDelta = rectTransform.sizeDelta,
				AnchorMin = rectTransform.anchorMin,
				AnchorMax = rectTransform.anchorMax,
				AnchoredPosition = rectTransform.anchoredPosition,
				Pivot = rectTransform.pivot
			};
		}
		
		public static void ApplyRectTransformData(this RectTransform rectTransform, RectTransformData rectTransformData)
		{
			if (rectTransformData == null)
			{
				return;
			}
			
			rectTransform.localPosition = rectTransformData.LocalPosition;
			rectTransform.localScale = rectTransformData.LocalScale;
			rectTransform.sizeDelta = rectTransformData.SizeDelta;
			rectTransform.anchorMin = rectTransformData.AnchorMin;
			rectTransform.anchorMax = rectTransformData.AnchorMax;
			rectTransform.anchoredPosition = rectTransformData.AnchoredPosition;
			rectTransform.pivot = rectTransformData.Pivot;
		}
		
		public static void SetAnchorsInCenter(this RectTransform rectTransform)
		{
			rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
			rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
		}
	}
	public class RectTransformData
	{
		public Vector3 LocalPosition { get; set; }
		
		public Vector3 LocalScale { get; set; }
		
		public Vector2 SizeDelta { get; set; }
		
		public Vector2 AnchorMin { get; set; }
		
		public Vector2 AnchorMax { get; set; }
		
		public Vector2 AnchoredPosition { get; set; }
		
		public Vector2 Pivot { get; set; }
	}
}