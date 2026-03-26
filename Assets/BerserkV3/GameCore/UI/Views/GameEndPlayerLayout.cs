using System.Threading;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using RR.UI.FrameSystem;

namespace BerserkV3.GameCore.UI
{
	public partial class GameEndPlayerLayout : BaseView
	{
		private const string DefaultMaskArtId = "Vulcanite_Mask";
		private const string DefaultAvatarArtId = "MAMOUTH_1613473479";
		private const string DefaultFrameArtId = "Vulcanite_Border_Neutral";

		private bool releseMaskImage;
		private bool releseAvatarImage;
		private bool releseFrameImage;

		public async UniTask SetArtMaskAsync(string artId, CancellationToken token = default)
		{
			await LoadWithFallbackAsync(MaskImage, artId, DefaultMaskArtId, releseMaskImage, token);
			releseMaskImage = true;
		}

		public async UniTask SetArtAsync(string artId, CancellationToken token = default)
		{
			await LoadWithFallbackAsync(AvatarImage, artId, DefaultAvatarArtId, releseAvatarImage, token);
			releseAvatarImage = true;
		}

		public async UniTask SetFrameArtAsync(string artId, CancellationToken token = default)
		{
			await LoadWithFallbackAsync(FrameImage, artId, DefaultFrameArtId, releseFrameImage, token);
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

		private static async UniTask LoadWithFallbackAsync(UnityEngine.Object target, string artId, string fallbackArtId, bool releasePrevious, CancellationToken token)
		{
			var primaryId = string.IsNullOrWhiteSpace(artId) ? fallbackArtId : artId;
			try
			{
				await target.LoadResourceAsync(primaryId, relesePrevious: releasePrevious, token: token);
			}
			catch
			{
				if (primaryId == fallbackArtId || token.IsCancellationRequested)
					throw;

				await target.LoadResourceAsync(fallbackArtId, relesePrevious: releasePrevious, token: token);
			}
		}
	}
}
