using Berserk.Shared.Data.Customisation;
using BerserkV3.GameCore.Customisations;
using BerserkV3.Generic.Customisation;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using RR.UI.FrameSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace BerserkV3.GameCore.UI
{
	public partial class BackgroundView : BaseView
	{
		[SerializeField] private RawImage BackgroundImage;
		[SerializeField] private RawImage Plane;
		[SerializeField] private RawImage BottomLeftCorner;
		[SerializeField] private RawImage BottomRightCorner;
		[SerializeField] private RawImage TopLeftCorner;
		[SerializeField] private RawImage TopRightCorner;

		[Inject]
		private void Construct()
		{
			GameCustomisationsAdapter.Application.SubscribeOnReady(LoadCustomisations);
		}

		private async UniTask LoadCustomisations()
		{
			var token = this.GetCancellationTokenOnDestroy();
			var gameboardAssetData = GameCustomisationsAdapter.Application
				.GetSelf<GameboardAssetData>(CustomisationType.Gameboard);
			
			var backgroundAssetData = GameCustomisationsAdapter.Application
				.GetSelf<AssetData>(CustomisationType.Background);

			await UniTask.WhenAll(
				BackgroundImage.LoadResourceAsync(backgroundAssetData.URL, token),
				Plane.LoadResourceAsync(gameboardAssetData.URL, token),
				BottomLeftCorner.LoadResourceAsync(gameboardAssetData.BottomLeftCornerUrl, token),
				BottomRightCorner.LoadResourceAsync(gameboardAssetData.BottomRightCornerUrl, token),
				TopLeftCorner.LoadResourceAsync(gameboardAssetData.TopLeftCornerUrl, token),
				TopRightCorner.LoadResourceAsync(gameboardAssetData.TopRightCornerUrl, token))
				.AttachExternalCancellation(token);
		}

		private void OnDestroy()
		{
			Plane.ReleaseResource();
			BottomLeftCorner.ReleaseResource();
			BottomRightCorner.ReleaseResource();
			TopLeftCorner.ReleaseResource();
			TopRightCorner.ReleaseResource();
			BackgroundImage.ReleaseResource();
		}
	}
}
