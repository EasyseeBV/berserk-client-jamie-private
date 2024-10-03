using System;
using System.Linq;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UI.Banners
{

	public sealed class BannersView : MonoBehaviour
	{
		[SerializeField] private string[] bannerKeys = Array.Empty<string>();
		[SerializeField] private BannerWidget[] bannerWidgets = Array.Empty<BannerWidget>();

		private CancellationTokenSource updateSource;
		private void Start()
		{
			if (bannerKeys.Length == 0)
				bannerKeys = bannerWidgets.Select(x => x.gameObject.name).ToArray();
			
			UpdateWidgets();
		}

		private void UpdateWidgets()
		{
			updateSource?.Cancel();
			updateSource?.Dispose();
			updateSource = new CancellationTokenSource();
			
			var shiftIndex = Random.Range(0, bannerKeys.Length);
			for (var i = 0; i < bannerWidgets.Length; i++)
			{
				var index = (shiftIndex + i) % bannerKeys.Length;
				bannerWidgets[i].Setup(bannerKeys[index], updateSource.Token);
			}
		}

		private void OnValidate()
		{
			if (bannerWidgets == null || bannerWidgets.Length == 0)
				bannerWidgets = GetComponentsInChildren<BannerWidget>(true);
		}

		private void OnDestroy()
		{
			updateSource?.Cancel();
			updateSource?.Dispose();
			updateSource = null;
		}
	}

}