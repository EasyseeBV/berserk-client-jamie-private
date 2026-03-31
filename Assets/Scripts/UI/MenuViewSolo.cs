using System.Collections.Generic;
using Berserk.Shared.Data.Enums;
using TMPro;
using UnityEngine;

namespace UI
{
	public partial class MenuView
	{
		private ButtonView _soloBtn;

		private void InitSoloMenuShell()
		{
			GauntletMatchPresentation.Clear();
			ArenaThemeSettings.ClearRuntimeOverride();
			BoardLayoutSettings.ClearRuntimeOverride();
			EnsureLogoutButtonVisible();
			CreateSoloButton();
			ReflowMainMenuButtons();
		}

		private void EnsureLogoutButtonVisible()
		{
			if (LogOutBtn)
				LogOutBtn.gameObject.SetActive(true);
		}

		private void CreateSoloButton()
		{
			if (!PlayBtn || !DuelsBtn)
				return;

			if (_soloBtn)
				return;

			var clone = Object.Instantiate(DuelsBtn.gameObject, DuelsBtn.transform.parent);
			clone.name = "SoloBtn";
			clone.transform.SetSiblingIndex(PlayBtn.transform.GetSiblingIndex() + 1);

			_soloBtn = clone.GetComponent<ButtonView>();
			_soloBtn.UnSubscribeAll();
			_soloBtn.Subscribe(ShowSoloAdventures);

			var label = clone.GetComponentInChildren<TextMeshProUGUI>(true);
			if (label)
				label.text = "SOLO";
		}

		private void ShowSoloAdventures()
		{
			ShowSoloAdventures(false);
		}

		public void ShowCampaignAdventures()
		{
			ShowCampaignAdventures(null);
		}

		public void ShowCampaignAdventures(Quadrant? quadrant)
		{
			var campaignView = CampaignModeView.EnsureInstance(this);
			campaignView.ApplyMenuBackground(FadeImg ? FadeImg.sprite : null);
			campaignView.SetPendingResult(null);
			campaignView.Show(this);
			if (quadrant.HasValue)
				campaignView.ShowQuadrantProgressView(quadrant.Value);
			else
				campaignView.ShowQuadrantSelection();
		}

		public void ShowCampaignAdventures(CampaignRedirectArg campaignResult)
		{
			var campaignView = CampaignModeView.EnsureInstance(this);
			campaignView.ApplyMenuBackground(FadeImg ? FadeImg.sprite : null);
			campaignView.SetPendingResult(campaignResult);
			campaignView.Show(this);
			campaignView.ShowQuadrantProgressView(campaignResult.Quadrant);
		}

		public void ShowSoloAdventures(bool openGauntletSelection)
		{
			GauntletMatchPresentation.Clear();
			ArenaThemeSettings.ClearRuntimeOverride();
			BoardLayoutSettings.ClearRuntimeOverride();
			var view = SoloAdventuresView.EnsureInstance(this);
			view.ApplyMenuBackground(FadeImg ? FadeImg.sprite : null);
			view.Show(this);
			if (openGauntletSelection)
				view.ShowGauntletSelectionView();
			else
				view.ShowMainModesView();
		}

		private void OnRectTransformDimensionsChange()
		{
			ReflowMainMenuButtons();
		}

		private void ReflowMainMenuButtons()
		{
			if (!PlayBtn || !DuelsBtn || !DeckBtn || !ProfileBtn)
				return;

			var parent = PlayBtn.transform.parent as RectTransform;
			if (!parent)
				return;

			var buttons = new List<ButtonView>
			{
				PlayBtn,
				_soloBtn,
				DuelsBtn,
				DeckBtn,
				ProfileBtn,
				LogOutBtn
			};

			buttons.RemoveAll(button => !button || !button.gameObject.activeSelf);
			if (buttons.Count == 0)
				return;

			var maxHeight = parent.rect.height;
			if (maxHeight <= 0f)
				return;

			const float baseSpacing = 10f;
			const float verticalPadding = 18f;
			var baseHeights = new List<float>(buttons.Count);
			var totalBaseHeight = 0f;

			foreach (var button in buttons)
			{
				var rect = button.transform as RectTransform;
				var height = rect ? Mathf.Max(1f, rect.sizeDelta.y) : 46f;
				baseHeights.Add(height);
				totalBaseHeight += height;
			}

			var naturalHeight = totalBaseHeight + (buttons.Count - 1) * baseSpacing + verticalPadding * 2f;
			var scale = Mathf.Clamp((maxHeight - verticalPadding * 2f) / Mathf.Max(naturalHeight, 1f), 0.82f, 1f);
			var spacing = baseSpacing * scale;
			var scaledTotalHeight = 0f;
			for (var i = 0; i < baseHeights.Count; i++)
				scaledTotalHeight += baseHeights[i] * scale;
			scaledTotalHeight += (buttons.Count - 1) * spacing;

			var cursorY = scaledTotalHeight * 0.5f;
			for (var i = 0; i < buttons.Count; i++)
			{
				var button = buttons[i];
				var rect = button.transform as RectTransform;
				if (!rect)
					continue;

				var height = baseHeights[i] * scale;
				rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
				rect.pivot = new Vector2(0.5f, 0.5f);
				rect.localScale = Vector3.one * scale;
				rect.anchoredPosition = new Vector2(0f, cursorY - height * 0.5f);
				cursorY -= height + spacing;
			}
		}
	}
}
