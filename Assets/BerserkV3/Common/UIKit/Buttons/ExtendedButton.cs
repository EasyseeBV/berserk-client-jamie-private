using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BerserkV3.Common.UIKit
{
	public class ExtendedButton : Button
	{
		[SerializeField] public ButtonClickedEvent onEnter = new();
		[SerializeField] public ButtonClickedEvent onExit = new();
		
		public override void OnPointerEnter(PointerEventData eventData)
		{
			base.OnPointerEnter(eventData);
			onEnter?.Invoke();
		}

		public override void OnPointerExit(PointerEventData eventData)
		{
			base.OnPointerExit(eventData);
			onExit?.Invoke();
		}

		public void UnscribeAll()
		{
			onClick.RemoveAllListeners();
		}
		
		public void UnscribeAllExtended()
		{
			onEnter.RemoveAllListeners();
			onExit.RemoveAllListeners();
		}
		
		protected override void OnDestroy()
		{
			base.OnDestroy();
			UnscribeAll();
		}
	}
}