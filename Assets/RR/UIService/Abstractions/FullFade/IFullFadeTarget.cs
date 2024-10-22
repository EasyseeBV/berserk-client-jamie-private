using UnityEngine;

namespace RR.UIService.FullFade
{
	public interface IFullFadeTarget
	{
		RectTransform Parent { get; }
		Color? FadeColor { get; }
		void OnFadeClick();
	}
}