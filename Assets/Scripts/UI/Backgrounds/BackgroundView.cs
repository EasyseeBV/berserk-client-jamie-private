using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using RR.UI.FrameSystem;
using UnityEngine.UI;

namespace UI
{
	public partial class BackgroundView : BaseView
	{
		protected override void OnAwake()
		{

		}
		
		private void SetArtAsync(string artUrl, Image target)
		{
			target.LoadResourceAsync(artUrl).Forget();
		}

		private void OnDestroy()
		{
			Plane.ReleaseResource();
			BackgroundImage.ReleaseResource();
			BottomLeftCorner.ReleaseResource();
			BottomRightCorner.ReleaseResource();
			TopLeftCorner.ReleaseResource();
			TopRightCorner.ReleaseResource();
		}
	}
}