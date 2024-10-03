namespace BerserkV3.Common.InputSystem.DragDropSystem
{
	public interface IDragSettings
	{
		/// <summary>
		/// Activity of this Draggable to drags.
		/// </summary>
		bool Enable {get; set;}
		
		/// <summary>
		/// Whether the drag system should reset the position and other settings when canceling a drag to the original before the drag.
		/// </summary>
		bool ResetOnCanceled {get;}
		
		/// <summary>
		/// Enable changing transparency when hovering over an available drop location.
		/// </summary>
		bool InOutTransparency {get;}
		
		/// <summary>
		/// Transparency value when Drop is not available for the current drag.
		/// </summary>
		float TransparencyAmount {get;}
		
		/// <summary>
		/// At the start of the drag, the transformation of the parent element to the parent canvas will be changed.
		/// https://docs.unity3d.com/ScriptReference/Transform.SetParent.html
		/// </summary>
		bool WorldPositionStaysWhenDraggingStart { get; }
		
		/// <summary>
		/// Undoing the drag will change the transformation from the parent element to the old parent.
		/// Important : Only on drag cancel, not reset in IDropHander
		/// https://docs.unity3d.com/ScriptReference/Transform.SetParent.html
		/// </summary>
		bool WorldPositionStaysWhenDraggingCanceled { get; }
	}
}