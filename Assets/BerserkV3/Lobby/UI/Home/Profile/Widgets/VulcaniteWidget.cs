using System.Threading;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Lobby.UI.Home.Profile.Widgets
{
	public class VulcaniteWidget : MonoBehaviour
	{
		[SerializeField] private RawImage borderImage;
		[SerializeField] private RawImage artImage;
		
		private bool isLoaded = false;

		public async UniTask InitAsync(string heroArtUrl, string borderId, CancellationToken cancellationToken = default)
		{
			await UniTask.WhenAll(
				borderImage.LoadResourceAsync(borderId, cancellationToken, isLoaded), 
				artImage.LoadResourceAsync(heroArtUrl, cancellationToken, isLoaded))
				.AttachExternalCancellation(cancellationToken);
			isLoaded = true;
		}
		
		public void OnDestroy()
		{
			if (isLoaded)
			{
				artImage.ReleaseResource();
				borderImage.ReleaseResource();
			}

			isLoaded = false;
		}
	}
}