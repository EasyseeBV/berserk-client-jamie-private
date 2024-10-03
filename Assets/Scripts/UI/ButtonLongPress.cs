using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
	public class ButtonLongPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
	{
		[SerializeField]
		[Tooltip("How long must pointer be down on this object to trigger a long press")]
		private float holdTime = 1f;

		private bool isHeld = false;
		private bool isHolding = false;
		private float holdingTime;

		private Action onClick;
		private Action onLongPress;
		private Action onLongPressFinished;

		public void SubscribeOnClick(Action callback)
		{
			onClick += callback;
		}

		public void SubscribeOnLongPress(Action callback)
		{
			onLongPress += callback;
		}

		public void SubscribeOnLongPressFinished(Action callback)
		{
			onLongPressFinished += callback;
		}

		public void UnsubscribeOnClick(Action callback)
		{
			onClick -= callback;
		}

		public void UnsubscribeOnLongPress(Action callback)
		{
			onLongPress -= callback;
		}

		public void UnsubscribeOnLongPressFinished(Action callback)
		{
			onLongPressFinished -= callback;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			isHeld = false;
			isHolding = true;
			holdingTime = 0f;
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (isHolding)
				onLongPressFinished?.Invoke();

			isHolding = false;

			if (!isHeld)
				onClick?.Invoke();
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (isHolding)
				onLongPressFinished?.Invoke();

			isHolding = false;
		}

		private void Update()
		{
			if (isHolding && !isHeld)
			{
				holdingTime += Time.unscaledDeltaTime;

				if (!isHeld && holdingTime >= holdTime)
				{
					OnLongPress();
				}
			}
		}

		private void OnLongPress()
		{
			isHeld = true;
			onLongPress?.Invoke();
		}
	}
}