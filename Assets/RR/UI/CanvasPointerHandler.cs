using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RR.UI
{
	[RequireComponent(typeof(Canvas))]
	public class CanvasPointerHandler : MonoBehaviour, IPointerClickHandler
	{
		public event Action<PointerEventData> OnClickAction;

		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.pointerCurrentRaycast.gameObject == null)
				OnClickAction?.Invoke(eventData);
		}
	}
}