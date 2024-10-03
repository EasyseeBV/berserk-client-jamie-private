using System.Threading;
using BerserkV3.Common.LiveLinkRouter;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using UnityEngine.UI;
using UnityEngine;

namespace UI.Banners
{

	public sealed class BannerWidget : MonoBehaviour
	{
		[SerializeField] private RawImage bannerImage;
		[SerializeField] private Button bannerButton;
		[SerializeField] private bool isLeftOriented;

		private void Awake()
		{
			gameObject.SetActive(false);
		}

		public void Setup(string key, CancellationToken token)
		{
			bannerButton.onClick.RemoveAllListeners();
			bannerButton.onClick.AddListener(() => LiveLinkRouterAdapter.Service.OpenLinkByKey(key));
			bannerImage.LoadResourceAsync(GetArtKey(key), token)
				.ContinueWith(() => gameObject.SetActive(true));
		}

		private void OnDestroy()
		{
			if (bannerButton)
				bannerButton.onClick.RemoveAllListeners();
			
			if (bannerImage)
				bannerImage.ReleaseResource();
		}

		private string GetArtKey(string key)
		{
			return key + (isLeftOriented ? "_Left" : "_Right");
		}
	}

}