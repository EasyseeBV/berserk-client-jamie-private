using BerserkV3.Common.PreviewSystem;
using RR.UI.FrameSystem;

namespace BerserkV3.Lobby.UI
{
	public partial class DeckValuePreviewView : BaseView
	{
		public void Show(IPreviewData prevewData, bool noAnimation = true)
		{
			if (prevewData is not DeckValueInfoData data) 
				return;

			Title.SetText(data.Title);
			Description.SetText(data.Description);
			base.Show(noAnimation:noAnimation);
		}

		public void Close(bool noAnimation = true)
		{
			base.Close(noAnimation:noAnimation);
		}
	}
}