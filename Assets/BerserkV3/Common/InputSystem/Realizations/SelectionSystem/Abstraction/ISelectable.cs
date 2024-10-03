using UnityEngine;

namespace BerserkV3.Common.InputSystem.SelectionSystem
{
	public interface ISelectable
	{
		/// <summary>
		/// Target of some reference view
		/// </summary>
		GameObject TargetView { get; }

		bool CanSelect();
	}
}