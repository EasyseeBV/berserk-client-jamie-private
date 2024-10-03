using UnityEngine;

namespace BerserkV3.Common.InputSystem.DragDropSystem
{
	public interface IDraggable
	{
		/// <summary>
		/// Target of some reference view
		/// </summary>
		GameObject TargetView { get; }
		
		/// <summary>
		/// Individual settings drag and drop
		/// </summary>
		IDragSettings DragSettings {get;}
		
		/// <summary>
		/// Is it allowed to drag the current IDraggable.
		/// </summary>
		/// <returns>Uniq condition</returns>
		bool CanDrag();
		
		/// <summary>
		/// Is it allowed to drop the current IDraggable into the current IDropHandler.
		/// </summary>
		/// <param name="place">to Drop place</param>
		/// <returns></returns>
		bool CanDropIn(IDropHandler place);
	}
}