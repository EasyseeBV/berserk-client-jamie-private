using UnityEngine;

namespace BerserkV3.Generic.ChatWheel
{
	public interface IChatWheelSlot
	{
		string Id { get; }

		/// <summary>
		/// Existing entity on this slot
		/// </summary>
		IChatWheelElement Element { get; }

		RectTransform Container { get; }

		/// <summary>
		/// Set a new entity in the slot and release the previous entity from this slot.
		/// </summary>
		/// <param name="element">new item to set in slot or can be null if need release slot</param>
		/// <returns>released item or null if slot already empty</returns>
		IChatWheelElement Set(IChatWheelElement element);
	}
}