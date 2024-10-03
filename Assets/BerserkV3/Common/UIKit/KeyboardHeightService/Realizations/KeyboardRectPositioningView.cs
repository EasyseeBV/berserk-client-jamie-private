using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Common.UIKit.KeyboardHeightService
{
	[RequireComponent(typeof(RectTransform))]
	public class KeyboardRectPositioningView : MonoBehaviour
	{
		[SerializeField] private RectTransform movingView;
		[SerializeField] private RectTransform target;
		[SerializeField] private float additiveOffset = -50;
		
		private bool shouldRefresh;
		private bool shouldRecalculatePosition;
		private bool shouldResetPosition;

		private Vector2 defaultPosition;
		private RectTransform rectTranform;
		private RectTransform canvasRect;
		private float inputOffset;
		private float rate;

		private void Start()
		{
			if (GetComponentInParent<ContentSizeFitter>() || GetComponentInParent<LayoutGroup>())
				shouldRecalculatePosition = true;

			var canvas = GetComponentInParent<Canvas>().rootCanvas;
			var canvasScaler = canvas.GetComponent<CanvasScaler>();
			canvasRect = (RectTransform)canvas.transform;
			rectTranform = (RectTransform)transform;
			target = target ? target :rectTranform;
			rate = canvasScaler.referenceResolution.y / Screen.height;

			CalculatePosition();
			KeyboardHeightServiceAdapter.Service.OnFocuseChanged += OnKeyboardHeighServiceFocused;
			OnKeyboardHeighServiceFocused(KeyboardHeightServiceAdapter.Service.Focused);
		}
		
		private void Update()
		{
			if (shouldRefresh)
			{
				if (shouldRecalculatePosition) // it's fixing late UI positioning (everything that inherited from LayoutGroup init lately)
				{
					CalculatePosition();
					shouldRecalculatePosition = false;
				}

				var keyboardHeight = KeyboardHeightServiceAdapter.Service.KeyboardHeight * rate;
				
				if (keyboardHeight <= inputOffset)
				{
					if (!shouldResetPosition) 
						return;
					
					shouldResetPosition = false;
					movingView.anchoredPosition = defaultPosition;
					return;
				}
				
				var offset = keyboardHeight - inputOffset;
				movingView.anchoredPosition = new Vector2(movingView.anchoredPosition.x, defaultPosition.y + offset);
				shouldResetPosition = true;
			}
			else if (shouldResetPosition)
			{
				shouldResetPosition = false;
				movingView.anchoredPosition = defaultPosition;
			}
		}

		private void CalculatePosition()
		{
			defaultPosition = movingView.anchoredPosition;
			var inputPos = movingView.InverseTransformPoint(target.position);
			var currentPosY = canvasRect.rect.height * movingView.pivot.y;
			inputOffset = currentPosY + inputPos.y + additiveOffset;
		}

		private void OnKeyboardHeighServiceFocused(Object state)
		{
			shouldRefresh = KeyboardHeightServiceAdapter.Service.Focused == rectTranform;
		}

		private void OnDestroy()
		{
			if (KeyboardHeightServiceAdapter.Service != null)
				KeyboardHeightServiceAdapter.Service.OnFocuseChanged -= OnKeyboardHeighServiceFocused;
		}
	}

}