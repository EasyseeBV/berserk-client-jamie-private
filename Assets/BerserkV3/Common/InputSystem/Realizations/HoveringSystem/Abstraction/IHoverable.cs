using UnityEngine;

namespace BerserkV3.Common.InputSystem.HoveringSystem
{
	public interface IHoverable
	{
		/// <summary>
		/// Target of some reference view
		/// </summary>
		GameObject TargetView { get; }
		
		IHoverableSetting HoverableSetting { get; }
		
		int SublingIndex { get; set; }
		
		Vector3 Size { get; set; }

		bool CanHover();
	}
}