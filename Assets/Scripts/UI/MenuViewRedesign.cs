using BerserkV3.Common.LiveLinkRouter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	/// <summary>
	/// Jamie's PBT UI Redesign — Bottom menu buttons (Shop, Community, Help)
	/// Creates buttons programmatically at runtime since prefab YAML injection is unreliable.
	/// Also enlarges the logo.
	/// </summary>
	public partial class MenuView
	{
		private const string SHOP_URL = "https://market.vulcanforged.com";
		private const string COMMUNITY_URL = "https://discord.com/invite/vulcan-forged";
		private const string HELP_URL = "https://vulcanforged.com";
		private const string BottomButtonsPanelName = "BottomButtonsPanel";

		private void InitRedesignButtons()
		{
			EnlargeLogo();
			if (!TryBindExistingBottomButtons())
				CreateBottomButtonsPanel();
		}

		private void EnlargeLogo()
		{
			if (LogoImg == null) return;
			var logoRect = LogoImg.GetComponent<RectTransform>();
			if (logoRect == null) return;

			// Scale logo up ~40% from original (624x183 -> 875x257)
			logoRect.sizeDelta = new Vector2(875f, 257f);
			// Move it down slightly to account for larger size
			logoRect.anchoredPosition = new Vector2(0f, -20f);
		}

		private void CreateBottomButtonsPanel()
		{
			var panelGO = new GameObject(BottomButtonsPanelName, typeof(RectTransform), typeof(HorizontalLayoutGroup));
			panelGO.transform.SetParent(transform, false);

			var panelRect = panelGO.GetComponent<RectTransform>();
			// Anchor to bottom center
			panelRect.anchorMin = new Vector2(0.5f, 0f);
			panelRect.anchorMax = new Vector2(0.5f, 0f);
			panelRect.pivot = new Vector2(0.5f, 0f);
			panelRect.anchoredPosition = new Vector2(0f, 20f);
			panelRect.sizeDelta = new Vector2(500f, 70f);

			var layout = panelGO.GetComponent<HorizontalLayoutGroup>();
			layout.spacing = 40f;
			layout.childAlignment = TextAnchor.MiddleCenter;
			layout.childForceExpandWidth = false;
			layout.childForceExpandHeight = false;
			layout.childControlWidth = false;
			layout.childControlHeight = false;

			CreateMenuButton(panelGO.transform, "ShopBtn", "SHOP", SHOP_URL);
			CreateMenuButton(panelGO.transform, "CommunityBtn", "COMMUNITY", COMMUNITY_URL);
			CreateMenuButton(panelGO.transform, "HelpBtn", "HELP", HELP_URL);
		}

		private bool TryBindExistingBottomButtons()
		{
			var panel = transform.Find(BottomButtonsPanelName);
			if (panel == null)
				return false;

			BindExistingButton(panel, "ShopBtn", SHOP_URL);
			BindExistingButton(panel, "CommunityBtn", COMMUNITY_URL);
			BindExistingButton(panel, "HelpBtn", HELP_URL);
			return true;
		}

		private static void BindExistingButton(Transform parent, string buttonName, string url)
		{
			if (parent == null)
				return;

			var buttonTransform = parent.Find(buttonName);
			if (buttonTransform == null)
				return;

			var button = buttonTransform.GetComponent<Button>();
			if (button == null)
				return;

			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(() => OpenExternalUrlStatic(url));
		}

		private void CreateMenuButton(Transform parent, string name, string label, string url)
		{
			// Button container
			var btnGO = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
			btnGO.transform.SetParent(parent, false);

			var btnRect = btnGO.GetComponent<RectTransform>();
			btnRect.sizeDelta = new Vector2(130f, 50f);

			// Semi-transparent dark background
			var btnImage = btnGO.GetComponent<Image>();
			btnImage.color = new Color(0.1f, 0.08f, 0.06f, 0.85f);

			// Add rounded look via sprite type
			var btn = btnGO.GetComponent<Button>();
			var colors = btn.colors;
			colors.normalColor = new Color(0.1f, 0.08f, 0.06f, 0.85f);
			colors.highlightedColor = new Color(0.9f, 0.5f, 0.1f, 0.9f); // Orange highlight on hover
			colors.pressedColor = new Color(1f, 0.6f, 0.2f, 1f);
			colors.selectedColor = colors.normalColor;
			btn.colors = colors;

			// Wire URL
			btn.onClick.AddListener(() => OpenExternalUrl(url));

			// Text label
			var textGO = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
			textGO.transform.SetParent(btnGO.transform, false);

			var textRect = textGO.GetComponent<RectTransform>();
			textRect.anchorMin = Vector2.zero;
			textRect.anchorMax = Vector2.one;
			textRect.sizeDelta = Vector2.zero;
			textRect.offsetMin = Vector2.zero;
			textRect.offsetMax = Vector2.zero;

			var tmp = textGO.GetComponent<TextMeshProUGUI>();
			tmp.text = label;
			tmp.fontSize = 16f;
			tmp.alignment = TextAlignmentOptions.Center;
			tmp.color = new Color(0.95f, 0.85f, 0.6f, 1f); // Gold text
			tmp.fontStyle = FontStyles.Bold;
		}

		private void OpenExternalUrl(string url)
		{
			OpenExternalUrlStatic(url);
		}

		private static void OpenExternalUrlStatic(string url)
		{
			if (LiveLinkRouterAdapter.Service != null)
			{
				LiveLinkRouterAdapter.Service.OpenLink(url);
			}
			else
			{
				Application.OpenURL(url);
			}
		}
	}
}
