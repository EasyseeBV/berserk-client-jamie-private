using System;

namespace BerserkV3.Common.InputSystem.HoveringSystem
{
	public interface IHoveringSystem
	{
		event Action<IHoverable> OnHoverEnter;

		event Action<IHoverable> OnHoverExit;
		
		bool Enabled { get; }
		
		void Registration(IHoverable hoverable);
		
		void UnRegistration(IHoverable hoverable);

		void Enable(bool value);

		void Start(IHoverable hoverable);
		
		void Cancel(IHoverable hoverable = null);

		void Clear();
	}
}