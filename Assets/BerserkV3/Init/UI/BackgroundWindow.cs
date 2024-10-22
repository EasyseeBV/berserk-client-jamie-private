using System.Threading;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using RR.UIService;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Init.UI
{
	public class BackgroundWindow : UIWindowBase
	{
		[SerializeField] protected RawImage image;
		private Texture defaultTexture;
		private bool releasePrevious;

		protected override void OnInit()
		{
			defaultTexture = image.texture;
			base.OnInit();
		}

		public void SetImage(Texture value)
		{
			if (releasePrevious)
				image.ReleaseResource();

			releasePrevious = false;
			image.texture = value;
		}

		public async UniTask SetImageAsync(string url, CancellationToken token = default)
		{
			await image.LoadResourceAsync(url, token, releasePrevious);
			releasePrevious = true;
		}

		protected override void OnDisposed()
		{
			if (releasePrevious)
				image.ReleaseResource();
			
			releasePrevious = false;
			base.OnDisposed();
		}

		public void SetDefaultImage()
		{
			SetImage(defaultTexture);
		}
	}
}