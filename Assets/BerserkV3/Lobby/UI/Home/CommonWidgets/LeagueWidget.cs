using System.Threading;
using BerserkV3.Common.UIKit;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Lobby.UI.Home.CommonWidgets
{
	public class LeagueWidget : UIViewBase
	{
		[SerializeField] private RawImage flagArtImage;
		private bool releasePrevious;

		public async UniTask InitAsync(string flagArtUrl, CancellationToken token = default)
		{
			await flagArtImage.LoadResourceAsync(flagArtUrl, token, releasePrevious);
			releasePrevious = true;
		}

		public void Clear()
		{
			if (releasePrevious)
				flagArtImage.ReleaseResource();

			releasePrevious = false;
		}

		private void OnDestroy()
		{
			Clear();
		}
	}
}