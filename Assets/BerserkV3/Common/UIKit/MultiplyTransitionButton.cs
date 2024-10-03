using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Common.UIKit
{
	public class MultiplyTransitionButton : Button
	{
		protected override void DoStateTransition(SelectionState state, bool instant)
		{
			var targetColor = state switch
			{
				SelectionState.Disabled => colors.disabledColor,
				SelectionState.Highlighted => colors.highlightedColor,
				SelectionState.Normal => colors.normalColor,
				SelectionState.Pressed => colors.pressedColor,
				SelectionState.Selected => colors.selectedColor,
				_ => Color.white
			};

			var graphics = TryGetComponent(out GraphicsCollector graphicsCollector)
				? graphicsCollector.TargetGraphics 
				: GetComponentsInChildren<Graphic>();

			foreach (var graphic in graphics)
			{
				if (graphic && graphic.gameObject.activeSelf)
					graphic.CrossFadeColor(targetColor, instant ? 0f : colors.fadeDuration, true, true);
			}
		}
	}
}