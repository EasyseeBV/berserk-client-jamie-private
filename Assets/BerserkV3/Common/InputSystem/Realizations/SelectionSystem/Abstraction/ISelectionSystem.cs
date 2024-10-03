using System;

namespace BerserkV3.Common.InputSystem.SelectionSystem
{
	public interface ISelectionSystem
	{
		event Action<ISelectable> OnSelected;
		event Action<ISelectable> OnCanceled;
		event Action<ISelectable> OnCanceledBySelection;
		
		ISelectable Current { get; }

		void Registration(ISelectable selectable);
		
		void UnRegistration(ISelectable selectable);
		
		void Select(ISelectable selectable);
		
		void Cancel();
		
		void Clear();
	}
}