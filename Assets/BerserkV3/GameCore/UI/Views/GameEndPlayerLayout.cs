using System.Threading;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using RR.UI.FrameSystem;

namespace BerserkV3.GameCore.UI
{
	public partial class GameEndPlayerLayout : BaseView
	{
		private bool releseMaskImage;
		private bool releseAvatarImage;
		private bool releseFrameImage;

		public async UniTask SetArtMaskAsync(string artId, CancellationToken token = default)
		{
			if (string.IsNullOrEmpty(artId))
				return;
			
			await MaskImage.LoadResourceAsync(artId, relesePrevious: releseMaskImage, token: token);
			releseMaskImage = true;
		}

		public async UniTask SetArtAsync(string artId, CancellationToken token = default)
		{
			if (string.IsNullOrEmpty(artId))
				return;
			
			await AvatarImage.LoadResourceAsync(artId, relesePrevious: releseAvatarImage, token: token);
			releseAvatarImage = true;
		}

		public async UniTask SetFrameArtAsync(string artId, CancellationToken token = default)
		{
			if (string.IsNullOrEmpty(artId))
				return;

			await FrameImage.LoadResourceAsync(artId, relesePrevious: releseFrameImage, token: token);
			releseFrameImage = true;
		}

		public void SetStateText(string value)
		{
			if (StateText)
				StateText.SetText(value);
		}
		
		public void Clear()
		{
			if (releseMaskImage)
				MaskImage.ReleaseResource();
			
			if (releseAvatarImage)
				AvatarImage.ReleaseResource();

			if (releseFrameImage)
				FrameImage.ReleaseResource();

			releseMaskImage = false;
			releseAvatarImage = false;
			releseFrameImage = false;
		}
		
		private void OnDestroy()
		{
			Clear();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			Clear();
		}
	}
}