using System;
using System.Collections.Generic;
using UnityEngine;

namespace BerserkV3.Generic.ChatWheel
{
	public interface IChatWheel
	{
		event Action<bool> OnFocus;
		event Action<IChatWheelSlot> OnSlotClick;
		event Action<IChatWheelSlot> OnSlotHovered;
		event Action<IChatWheelSlot, IChatWheelElement> OnDroppedIn;
		
		RectTransform Container { get; }
		int SlotCount { get; }

		void Init(int slotCount);
		void Add(params IChatWheelElement[] elements);
		void Show(bool force = false, Action onShowed = null);
		void Close(bool force = false, Action onClosed = null);
		void Clear();

		void SwapSlotsElement(string fromSlotId, string toSlotId);
		IChatWheelElement ReplaceSlotElement(string slotId, IChatWheelElement element);
		IEnumerable<IChatWheelElement> Set(params IChatWheelElement[] elements);
		IEnumerable<IChatWheelElement> GetSlotElements();
		IEnumerable<IChatWheelSlot> GetSlots();
		IEnumerable<IChatWheelSlot> GetOccupiedSlots();
	}
}