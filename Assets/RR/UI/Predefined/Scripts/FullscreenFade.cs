using RR.UI.FrameSystem;

namespace RR.UI.Predefined.Scripts
{
	public class FullscreenFade : BaseView
	{
		protected override void OnDisable()
		{
			Destroy(gameObject);
			base.OnDisable();
		}
	}
}
