using UnityEngine;

namespace BerserkV3.GameCore.TargetSystem.Abstraction
{
	public interface ITargetPickerView
	{
		bool Enabled { get; }

		void Enable(Transform fromTaget);
		
		void Disable();
		
		void Drag(Vector3 position);
	}
}