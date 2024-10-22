using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Common.UIKit
{
	public class MultiplyTransitionButton : Button
	{
		protected override void DoStateTransition(SelectionState state, bool instant)
		{
			base.DoStateTransition(state, instant);
			if (!gameObject.activeInHierarchy)
                return;
			
			var graphics = TryGetComponent(out GraphicsCollector graphicsCollector)
				? graphicsCollector.TargetGraphics 
				: GetComponentsInChildren<Graphic>();
			
			if (graphics is not {Length: > 0})
				return;

            Color tintColor;
            Sprite transitionSprite;
            string triggerName;

            switch (state)
            {
                case SelectionState.Normal:
                    tintColor = colors.normalColor;
                    transitionSprite = null;
                    triggerName = animationTriggers.normalTrigger;
                    break;
                case SelectionState.Highlighted:
                    tintColor = colors.highlightedColor;
                    transitionSprite = spriteState.highlightedSprite;
                    triggerName = animationTriggers.highlightedTrigger;
                    break;
                case SelectionState.Pressed:
                    tintColor = colors.pressedColor;
                    transitionSprite = spriteState.pressedSprite;
                    triggerName = animationTriggers.pressedTrigger;
                    break;
                case SelectionState.Selected:
                    tintColor = colors.selectedColor;
                    transitionSprite = spriteState.selectedSprite;
                    triggerName = animationTriggers.selectedTrigger;
                    break;
                case SelectionState.Disabled:
                    tintColor = colors.disabledColor;
                    transitionSprite = spriteState.disabledSprite;
                    triggerName = animationTriggers.disabledTrigger;
                    break;
                default:
                    tintColor = Color.black;
                    transitionSprite = null;
                    triggerName = string.Empty;
                    break;
            }
            
			foreach (var graphic in graphics)
			{
				switch (transition)
				{
					case Transition.ColorTint or Transition.SpriteSwap:
						StartColorTween(graphic, tintColor * colors.colorMultiplier, instant);
						break;
					// case Transition.SpriteSwap:
					// 	DoSpriteSwap(graphic.GetComponent<Image>(), transitionSprite);
					// 	break;
					case Transition.Animation:
						TriggerAnimation(graphic.GetComponent<Selectable>(), triggerName);
						break;
				}
			}
		}

		private void StartColorTween(Graphic graphic, Color targetColor, bool instant)
		{
			if (!graphic)
				return;

			graphic.CrossFadeColor(targetColor, instant ? 0f : colors.fadeDuration, true, true);
		}

		private void DoSpriteSwap(Image graphic, Sprite newSprite)
		{
			if (!graphic)
				return;

			graphic.overrideSprite = newSprite;
		}

		private void TriggerAnimation(Selectable selectable, string triggername)
		{
			if (!selectable || !selectable.animator || !selectable.animator.isActiveAndEnabled)
				return;
			
			var selectableAnimator = selectable.animator;
			if (transition != Transition.Animation || !selectableAnimator.isActiveAndEnabled || !selectableAnimator.hasBoundPlayables || string.IsNullOrEmpty(triggername))
				return;

			selectableAnimator.ResetTrigger(animationTriggers.normalTrigger);
			selectableAnimator.ResetTrigger(animationTriggers.highlightedTrigger);
			selectableAnimator.ResetTrigger(animationTriggers.pressedTrigger);
			selectableAnimator.ResetTrigger(animationTriggers.selectedTrigger);
			selectableAnimator.ResetTrigger(animationTriggers.disabledTrigger);

			selectableAnimator.SetTrigger(triggername);
		}
	}
}