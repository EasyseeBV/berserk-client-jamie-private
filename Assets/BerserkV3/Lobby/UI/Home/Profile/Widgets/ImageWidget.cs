using System.Threading;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Lobby.UI.Home.Profile.Widgets
{
	public class ImageWidget : MonoBehaviour
	{
		[SerializeField] private RawImage artImage;
		
		private bool isLoaded = false;

		public async UniTask InitAsync(string artUrl, CancellationToken cancellationToken = default)
		{
			await artImage.LoadResourceAsync(artUrl, cancellationToken, isLoaded);
			isLoaded = true;
		}

		public void OnDestroy()
		{
			if (isLoaded)
				artImage.ReleaseResource();

			isLoaded = false;
		}
	}
}