using RR.UI.FrameSystem;

namespace UI
{
	public interface ICustomisationDraggableView : ICustomisationItemView
	{
		void SetFactory(ICustomisationViewFactory factory);
		void SetDragMode(DragMode dragMode);
	}
}