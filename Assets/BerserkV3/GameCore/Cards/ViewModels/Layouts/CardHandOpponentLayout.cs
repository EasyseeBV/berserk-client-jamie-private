using System.Threading;
using Berserk.Shared.Data.Customisation;
using BerserkV3.GameCore.Customisations;
using BerserkV3.Generic.Customisation;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;

namespace BerserkV3.GameCore.Cards
{
	public partial class CardHandOpponentLayout : BaseCardLayout
	{
		private bool previouseCommonRelease;

		protected override async UniTask OnEnabledAsync(CancellationToken token)
		{
			var assetData = GameCustomisationsAdapter.Application
				.Get<AssetData>(RuntimeData.OwnerUserId, CustomisationType.CardBack);
			
			await UniTask.WhenAll(base.OnEnabledAsync(token), SetArtAsync(assetData.URL, token));
			previouseCommonRelease = true;
		}

		protected override void OnDisabled()
		{
			if (!previouseCommonRelease)
				return;

			previouseCommonRelease = false;
			base.OnDisabled();
			ArtImage.ReleaseResource();
		}
		
		private UniTask SetArtAsync(string artUrl, CancellationToken token = default)
		{
			return ArtImage.LoadResourceAsync(artUrl, token, previouseCommonRelease);
		}
	}
}