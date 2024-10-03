using UnityEngine;

namespace RR.Game.TutorialSystem.Presentation.UI
{
	public class RectTransformHintTarget : HintTarget
	{
		private RectTransform rectTransform;

		public Vector2 SizeDelta => rectTransform.sizeDelta;
		public Vector2 AnchoredPosition => rectTransform.anchoredPosition;

		public override bool IsClickToCloseHint => TargetButtons is {Length: > 0};

		protected override void OnAwake()
		{
			rectTransform = GetComponent<RectTransform>();
		}
	}
}