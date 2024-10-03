using RR.UI.FrameSystem;

namespace BerserkV3.Common.TutorialSystem
{

	public partial class UITutorialBlockRaycasts : BaseView
	{
		public void Enable(bool value)
		{
			gameObject.SetActive(value);
		}
	}

}