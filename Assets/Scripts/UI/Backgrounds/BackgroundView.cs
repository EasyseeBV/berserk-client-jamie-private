using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using RR.UI.FrameSystem;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public partial class BackgroundView : BaseView
	{
		protected override void OnAwake()
		{
			ApplyArenaTheme();
			ArenaThemeSettings.Changed += OnArenaThemeChanged;
		}

		private void OnArenaThemeChanged()
		{
			ApplyArenaTheme();
		}

		private void ApplyArenaTheme()
		{
			var theme = ArenaThemeSettings.Current;

			StretchToFill(transform as RectTransform);
			StretchToFill(BackgroundImage ? BackgroundImage.rectTransform : null);

			if (BackgroundImage)
			{
				BackgroundImage.preserveAspect = false;
				BackgroundImage.enabled = true;
				SetArtAsync(theme.BackgroundResourceId, BackgroundImage);
			}

			if (Plane)
			{
				Plane.enabled = true;
				SetArtAsync(theme.GameboardResourceId, Plane);
			}

			if (BottomLeftCorner)
			{
				BottomLeftCorner.enabled = true;
				SetArtAsync(theme.BottomLeftCornerResourceId, BottomLeftCorner);
			}

			if (BottomRightCorner)
			{
				BottomRightCorner.enabled = true;
				SetArtAsync(theme.BottomRightCornerResourceId, BottomRightCorner);
			}

			if (TopLeftCorner)
			{
				TopLeftCorner.enabled = true;
				SetArtAsync(theme.TopLeftCornerResourceId, TopLeftCorner);
			}

			if (TopRightCorner)
			{
				TopRightCorner.enabled = true;
				SetArtAsync(theme.TopRightCornerResourceId, TopRightCorner);
			}

			if (TableBase)
				TableBase.enabled = false;

			if (TableBorders)
				TableBorders.enabled = false;
		}

		private void SetArtAsync(string artUrl, Image target)
		{
			target.LoadResourceAsync(artUrl).Forget();
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
			Plane.ReleaseResource();
			BackgroundImage.ReleaseResource();
			BottomLeftCorner.ReleaseResource();
			BottomRightCorner.ReleaseResource();
			TopLeftCorner.ReleaseResource();
			TopRightCorner.ReleaseResource();
		}
	}
}
