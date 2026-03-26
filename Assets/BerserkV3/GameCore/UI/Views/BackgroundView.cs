using BerserkV3.GameCore.Customisations;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using RR.UI.FrameSystem;
using UI;
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
			ApplyFullscreenBackgroundLayout();
			GameCustomisationsAdapter.Application.SubscribeOnReady(LoadArenaTheme);
			ArenaThemeSettings.Changed += OnArenaThemeChanged;
			BoardLayoutSettings.Changed += OnBoardLayoutChanged;
		}

		private async UniTask LoadArenaTheme()
		{
			ApplyFullscreenBackgroundLayout();
			var token = this.GetCancellationTokenOnDestroy();
			var selectedTheme = ArenaThemeSettings.Current;

			await UniTask.WhenAll(
					BackgroundImage.LoadResourceAsync(selectedTheme.BackgroundResourceId, token),
					Plane.LoadResourceAsync(selectedTheme.GameboardResourceId, token),
					BottomLeftCorner.LoadResourceAsync(selectedTheme.BottomLeftCornerResourceId, token),
				BottomRightCorner.LoadResourceAsync(selectedTheme.BottomRightCornerResourceId, token),
				TopLeftCorner.LoadResourceAsync(selectedTheme.TopLeftCornerResourceId, token),
				TopRightCorner.LoadResourceAsync(selectedTheme.TopRightCornerResourceId, token))
				.AttachExternalCancellation(token);

			ApplyBoardLayoutVisibility();
		}

		private void OnArenaThemeChanged()
		{
			LoadArenaTheme().Forget();
		}

			private void OnBoardLayoutChanged()
			{
				ApplyBoardLayoutVisibility();
			}

			private void Update()
			{
				ApplyBoardLayoutVisibility();
			}

		private void ApplyFullscreenBackgroundLayout()
		{
			StretchToFill(transform as RectTransform);
			StretchToFill(BackgroundImage ? BackgroundImage.rectTransform : null);
			StretchToFill(Plane ? Plane.rectTransform : null);
		}

		private void ApplyBoardLayoutVisibility()
		{
			var isMinimal = BoardLayoutSettings.IsMinimal();
			SetGraphicVisible(Plane, !isMinimal);
			SetGraphicVisible(TableBase, !isMinimal);
			SetGraphicVisible(TableBorders, !isMinimal);
			ApplyCornerVisibility(!isMinimal);
		}

		private void ApplyCornerVisibility(bool isVisible)
		{
			SetGraphicVisible(BottomLeftCorner, isVisible);
			SetGraphicVisible(BottomRightCorner, isVisible);
			SetGraphicVisible(TopLeftCorner, isVisible);
			SetGraphicVisible(TopRightCorner, isVisible);
		}

		private static void SetGraphicVisible(Graphic graphic, bool isVisible)
		{
			if (!graphic)
				return;

			graphic.enabled = isVisible;

			if (graphic.gameObject.activeSelf != isVisible)
				graphic.gameObject.SetActive(isVisible);
		}

		private static void StretchToFill(RectTransform rectTransform)
		{
			if (!rectTransform)
				return;

			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.anchoredPosition = Vector2.zero;
			rectTransform.sizeDelta = Vector2.zero;
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
			rectTransform.localScale = Vector3.one;
		}

		private void OnDestroy()
		{
			ArenaThemeSettings.Changed -= OnArenaThemeChanged;
			BoardLayoutSettings.Changed -= OnBoardLayoutChanged;
			Plane.ReleaseResource();
			BottomLeftCorner.ReleaseResource();
			BottomRightCorner.ReleaseResource();
			TopLeftCorner.ReleaseResource();
			TopRightCorner.ReleaseResource();
			BackgroundImage.ReleaseResource();
		}
	}
}
