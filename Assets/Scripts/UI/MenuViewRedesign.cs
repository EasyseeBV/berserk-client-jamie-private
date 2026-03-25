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
		private const string BUTTON_BACKGROUND_RESOURCE = "UI/btn_background_fantasy";
		private const string SHOP_ICON_RESOURCE = "UI/icon_shop_v2";
		private const string COMMUNITY_ICON_RESOURCE = "UI/icon_community_v2";
		private const string HELP_ICON_RESOURCE = "UI/icon_help_v2";
		private const string SHOP_URL = "https://market.vulcanforged.com";
		private const string COMMUNITY_URL = "https://discord.com/invite/vulcan-forged";
		private const string HELP_URL = "https://vulcanforged.com";

		private void InitRedesignButtons()
		{
			EnsureFullBleedBackground();
			EnlargeLogo();
			CreateBottomButtonsPanel();
		}

		private void EnsureFullBleedBackground()
		{
			if (FadeImg == null)
				return;

			FadeImg.preserveAspect = false;

			var fadeRect = FadeImg.rectTransform;
			if (fadeRect == null)
				return;

			fadeRect.anchorMin = Vector2.zero;
			fadeRect.anchorMax = Vector2.one;
			fadeRect.pivot = new Vector2(0.5f, 0.5f);
			fadeRect.offsetMin = Vector2.zero;
			fadeRect.offsetMax = Vector2.zero;
			fadeRect.anchoredPosition = Vector2.zero;
			fadeRect.sizeDelta = Vector2.zero;
		}

		private void EnlargeLogo()
		{
			if (LogoImg == null)
				return;

			var logoRect = LogoImg.GetComponent<RectTransform>();
			if (logoRect == null)
				return;

			logoRect.sizeDelta = new Vector2(875f, 257f);
			logoRect.anchoredPosition = new Vector2(0f, -20f);
		}

		private void CreateBottomButtonsPanel()
		{
			var existing = transform.Find("BottomButtonsPanel");
			if (existing != null)
				Object.Destroy(existing.gameObject);

			var panelGO = new GameObject("BottomButtonsPanel", typeof(RectTransform), typeof(HorizontalLayoutGroup));
			panelGO.transform.SetParent(transform, false);

			var panelRect = panelGO.GetComponent<RectTransform>();
			panelRect.anchorMin = new Vector2(0.5f, 0f);
			panelRect.anchorMax = new Vector2(0.5f, 0f);
			panelRect.pivot = new Vector2(0.5f, 0f);
			panelRect.anchoredPosition = new Vector2(0f, 14f);
			panelRect.sizeDelta = new Vector2(760f, 84f);

			var layout = panelGO.GetComponent<HorizontalLayoutGroup>();
			layout.spacing = 28f;
			layout.childAlignment = TextAnchor.MiddleCenter;
			layout.childForceExpandWidth = false;
			layout.childForceExpandHeight = false;
			layout.childControlWidth = false;
			layout.childControlHeight = false;
			layout.padding = new RectOffset(8, 8, 6, 6);

			CreateMenuButton(panelGO.transform, "ShopBtn", "SHOP", SHOP_URL, SHOP_ICON_RESOURCE);
			CreateMenuButton(panelGO.transform, "CommunityBtn", "COMMUNITY", COMMUNITY_URL, COMMUNITY_ICON_RESOURCE);
			CreateMenuButton(panelGO.transform, "HelpBtn", "HELP", HELP_URL, HELP_ICON_RESOURCE);
		}

		private void CreateMenuButton(Transform parent, string name, string label, string url, string iconResource)
		{
			var btnGO = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(HorizontalLayoutGroup));
			btnGO.transform.SetParent(parent, false);

			var btnRect = btnGO.GetComponent<RectTransform>();
			btnRect.sizeDelta = new Vector2(180f, 60f);

			var btnImage = btnGO.GetComponent<Image>();
			btnImage.sprite = Resources.Load<Sprite>(BUTTON_BACKGROUND_RESOURCE);
			btnImage.type = Image.Type.Sliced;
			btnImage.color = Color.white;
			btnImage.raycastTarget = true;

			var btn = btnGO.GetComponent<Button>();
			var colors = btn.colors;
			colors.normalColor = new Color(1f, 1f, 1f, 1f);
			colors.highlightedColor = new Color(1f, 0.95f, 0.85f, 1f);
			colors.pressedColor = new Color(0.9f, 0.85f, 0.75f, 1f);
			colors.selectedColor = colors.normalColor;
			btn.colors = colors;
			btn.targetGraphic = btnImage;

			btn.onClick.AddListener(() => OpenExternalUrl(url));

			var layout = btnGO.GetComponent<HorizontalLayoutGroup>();
			layout.childAlignment = TextAnchor.MiddleCenter;
			layout.childControlWidth = false;
			layout.childControlHeight = false;
			layout.childForceExpandWidth = false;
			layout.childForceExpandHeight = false;
			layout.spacing = 8f;
			layout.padding = new RectOffset(14, 14, 8, 8);

			var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
			iconGO.transform.SetParent(btnGO.transform, false);

			var iconRect = iconGO.GetComponent<RectTransform>();
			iconRect.sizeDelta = new Vector2(40f, 40f);

			var iconImage = iconGO.GetComponent<Image>();
			iconImage.sprite = Resources.Load<Sprite>(iconResource);
			iconImage.preserveAspect = true;
			iconImage.color = Color.white;

			var textGO = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
			textGO.transform.SetParent(btnGO.transform, false);

			var textRect = textGO.GetComponent<RectTransform>();
			textRect.sizeDelta = new Vector2(102f, 40f);

			var tmp = textGO.GetComponent<TextMeshProUGUI>();
			tmp.text = label;
			tmp.fontSize = 18f;
			tmp.alignment = TextAlignmentOptions.MidlineLeft;
			tmp.color = new Color(0.95f, 0.85f, 0.6f, 1f);
			tmp.fontStyle = FontStyles.Bold;
			tmp.enableWordWrapping = false;
		}

		private void OpenExternalUrl(string url)
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
