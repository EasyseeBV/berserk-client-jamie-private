using System.Threading;
using BerserkV3.Common.UIKit;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Lobby.UI.Home.CommonWidgets
{
	public class CompactDeckWidget : UIViewBase
	{
		[SerializeField] private RawImage deckArtImage;
		[SerializeField] private RawImage quadrantArtImage;
		public bool Initialized { get; private set; }
		
		public async UniTask InitAsync(string deckArtUrl, string quadrantArtUrl, CancellationToken token = default)
		{
			await UniTask.WhenAll(
					deckArtImage.LoadResourceAsync(deckArtUrl, token, Initialized),
					quadrantArtImage.LoadResourceAsync(quadrantArtUrl, token, Initialized))
				.AttachExternalCancellation(token);

			Initialized = true;
		}

		public void Clear()
		{
			if(Initialized)
			{
				deckArtImage.ReleaseResource();
				quadrantArtImage.ReleaseResource();
			}

			Initialized = false;
		}

		private void OnDestroy()
		{
			Clear();
		}
	}
}
