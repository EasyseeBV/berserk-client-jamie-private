using System.Threading;
using BerserkV3.Generic.Customisation;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using RR.UI.FrameSystem;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class CustomisationItemView : BaseView, ICustomisationPreviewableView
	{
		[SerializeField] protected Button SelectionButton;
		[SerializeField] protected RawImage MainArtImage;
		[SerializeField] protected RawImage MainArtMaskImage;
		[SerializeField] protected GameObject SelectionImage;
		
		public CustomisationItem Current { get; private set; }
		
		public virtual async UniTask SetupAsync(CustomisationItem item, CancellationToken token = default)
		{
			SetActive(this, false);
			Current = item;
			SelectionButton.onClick.RemoveAllListeners();
			SelectionButton.onClick.AddListener(OnItemClicked);
			var mainArtTask = MainArtImage.LoadResourceAsync(GetMainArtUrl(), token);
			var maskArtTask = UniTask.CompletedTask;
			var maskUrl = GetMaskArtUrl();
			
			if (!string.IsNullOrEmpty(maskUrl))
				maskArtTask = MainArtMaskImage.LoadResourceAsync(maskUrl, token);

			await UniTask.WhenAll(mainArtTask, maskArtTask);
			token.ThrowIfCancellationRequested();
			Show(noAnimation:true);
		}

		public virtual void Deselect()
		{
			if (!SelectionImage)
				return;
			
			SelectionImage.SetActive(false);
		}

		public virtual void Select()
		{
			if (!SelectionImage)
				return;
			
			SelectionImage.SetActive(true);
		}
		
		public virtual void Dispose()
		{
			Close(noAnimation:true);
		}

		public virtual PreviewInfo GetPreviewInfo()
		{
			return new PreviewInfo
			{
				Type = Current.CustomisationType,
				PreviewAspect = GetAspectRatio(),
				PreviewUrl = GetMainArtUrl(),
			};
		}
		
		protected virtual string GetMainArtUrl()
		{
			return Current?.AssetData.URL;
		}

		protected virtual string GetMaskArtUrl()
		{
			return string.Empty;
		}

		private float GetAspectRatio(Rect? reference = null)
		{
			if (!MainArtImage)
				return 1f;

			reference ??= GetAspectReferenceRect();
			return reference.Value.width / reference.Value.height;
		}

		protected virtual Rect GetAspectReferenceRect()
		{
			if (!MainArtImage || !MainArtImage.rectTransform)
				return default;
			
			return MainArtImage.rectTransform.rect;
		}

		protected virtual void OnItemClicked()
		{
			Current?.Equip();
		}
		
		private void OnDestroy()
		{
			MainArtImage.ReleaseResource();
			MainArtMaskImage.ReleaseResource();
			if (SelectionButton)
				SelectionButton.onClick.RemoveAllListeners();
		}
	}
}