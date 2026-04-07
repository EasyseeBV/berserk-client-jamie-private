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
		[SerializeField] private GameObject _menuButtonPrefab;
		[SerializeField] private GameObject _generalButtonPrefab;
		[SerializeField] private GameObject _baseButtonPrefab;
		public GameObject BaseButtonPrefab => _baseButtonPrefab;
		public GameObject MenuButtonPrefab => _menuButtonPrefab;
		public GameObject GeneralButtonPrefab => _generalButtonPrefab;

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

		private void CreateMenuButton(Transform parent, string buttonName, string label, string url, string iconResource)
		{
			var btnGO = Instantiate(_menuButtonPrefab, parent, false);
			btnGO.name = buttonName;

			var btnRect = btnGO.GetComponent<RectTransform>();
			btnRect.sizeDelta = new Vector2(226f, 60f);

			var btn = btnGO.GetComponent<Button>();
			btn.onClick.AddListener(() => OpenExternalUrl(url));

			var iconImage = btnGO.transform.Find("Icon")?.GetComponent<Image>();
			if (iconImage != null)
				iconImage.sprite = Resources.Load<Sprite>(iconResource);

			var tmp = btnGO.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
			if (tmp != null)
				tmp.text = label;
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
