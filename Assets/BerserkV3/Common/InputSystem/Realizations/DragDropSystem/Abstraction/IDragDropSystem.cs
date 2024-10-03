using System;

namespace BerserkV3.Common.InputSystem.DragDropSystem
{
	public interface IDragDropSystem : IDisposable
	{
		/// <summary>
		/// When IDraggable to start a drag
		/// </summary>
		event Action<IDraggable> OnDragStart;

		/// <summary>
		/// When IDraggable update a drag
		/// </summary>
		event Action<IDraggable> OnDrag;

		/// <summary>
		/// When IDraggable target canceled a drag
		/// </summary>
		event Action<IDraggable, IDropHandler> OnDragCanceled;

		/// <summary>
		/// When IDraggable to overlap target an IDropHandler
		/// </summary>
		event Action<IDraggable, IDropHandler> OnDragIn;

		/// <summary>
		/// When IDraggable out of bounds an IDropHandler
		/// </summary>
		event Action<IDraggable, IDropHandler> OnDragOut;

		/// <summary>
		/// When IDraggable to dropped into an IDropHandler
		/// </summary>
		event Action<IDraggable, IDropHandler> OnDroppedIn;

		/// <summary>
		/// Current IDraggable object target to drag
		/// </summary>
		IDraggable Current { get; }

		/// <summary>
		/// Current Input handler
		/// </summary>
		IDragDropInputHandler InputHandler { get; }

		/// <summary>
		/// IsAny dragged now
		/// </summary>
		bool AnyDragged => Current != null;

		/// <summary>
		/// When IDraggable drag over IDropHandler
		/// </summary>
		IDropHandler CurrentDropHandler { get; }

		/// <summary>
		/// Subscribe to IDraggables to update drag and drop actions.
		/// </summary>
		/// <param name="value">Target draggable object</param>
		void Registration(IDraggable value);

		/// <summary>
		/// Subscribe to IDropHandlers to update drag and drop actions.
		/// </summary>
		/// <param name="value">Target drop handler object</param>
		void Registration(IDropHandler value);

		/// <summary>
		/// UnSubscribe your IDraggables async is executed at the end of the last update for this frame.
		/// </summary>
		/// <param name="value">Target draggable object</param>
		void UnRegistration(IDraggable value);

		/// <summary>
		/// UnSubscribe your IDropHandlers async is executed at the end of the last update for this frame.
		/// </summary>
		/// <param name="value">Target drop handler object</param>
		void UnRegistration(IDropHandler value);

		/// <summary>
		/// Cancel current drag
		/// </summary>
		void Cancel();

		void Clear();
	}
}