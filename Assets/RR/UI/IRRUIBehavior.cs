using UnityEngine;

namespace RR.UI
{
	public interface IRRUIBehaviour
	{
		RectTransform RectTransform { get; }
		CanvasGroup CanvasGroup { get; }

		void ResetPosition();
	}
}