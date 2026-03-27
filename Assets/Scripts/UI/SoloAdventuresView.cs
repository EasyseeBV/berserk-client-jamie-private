using System;
using System.Collections.Generic;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Lobby;
using BerserkV3.Common.Network;
using BerserkV3.Common.Utils;
using BerserkV3.Lobby.Deck;
using BerserkV3.Lobby.MatchMaking.Practice;
using BerserkV3.Lobby.Network;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using RR.Core.ResourceManagament;
using TMPro;
using RR.UI.FrameSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
	public class SoloAdventuresView : BaseView
	{
		private GameObject _menuButtonPrefab;
		private GameObject _generalButtonPrefab;

		private const string ButtonBackgroundResource = "UI/btn_background_fantasy";
		private const string CampaignCardResource = "UI/card_campaign";
		private const string GauntletCardResource = "UI/card_gauntlet";

		public static SoloAdventuresView Instance { get; private set; }

		private TMP_FontAsset _fontAsset;
		private Material _fontMaterial;
		private TextMeshProUGUI _titleLabel;
		private TextMeshProUGUI _gauntletProgressLabel;
		private TextMeshProUGUI _gauntletStarLabel;
		private TextMeshProUGUI _gauntletIntroLabel;
		private TextMeshProUGUI _gauntletHeaderLabel;
		private TextMeshProUGUI _gauntletCompletionBanner;
		private TextMeshProUGUI _backButtonLabel;
		private RectTransform _mainModesRoot;
		private RectTransform _gauntletSelectionRoot;
		private RectTransform _panelRoot;
		private Image _panelBackground;
		private bool _championVisualsLoaded;

		private readonly List<ChampionButtonState> _championButtons = new();
		private readonly List<TrackerNodeState> _trackerNodes = new();

		private sealed class ChampionButtonState
		{
			public string BotDeckId;
			public string Title;
			public Button Button;
			public Image Background;
			public Image AccentBar;
			public Image TierBadge;
			public TextMeshProUGUI TierLabel;
			public TextMeshProUGUI TitleLabel;
			public TextMeshProUGUI SubtitleLabel;
			public TextMeshProUGUI MetaLabel;
			public TextMeshProUGUI RewardLabel;
			public Image StatusChip;
			public TextMeshProUGUI StatusLabel;
			public Image LockShade;
			public Image ThemeStripe;
			public RawImage PortraitImage;
			public RawImage PortraitFrameImage;
		}

		private sealed class TrackerNodeState
		{
			public Image Fill;
			public TextMeshProUGUI Label;
			public TextMeshProUGUI Caption;
			public Image Connector;
		}

		public static SoloAdventuresView EnsureInstance(MenuView menuView)
		{
			if (Instance)
				return Instance;

			var parent = menuView.transform.parent as RectTransform;
			if (!parent)
				parent = menuView.GetComponentInParent<Canvas>()?.transform as RectTransform;

			var root = new GameObject(
				nameof(SoloAdventuresView),
				typeof(RectTransform),
				typeof(CanvasRenderer),
				typeof(Image),
				typeof(CanvasGroup),
				typeof(SoloAdventuresView));
			root.transform.SetParent(parent, false);

			var view = root.GetComponent<SoloAdventuresView>();
			view._menuButtonPrefab = menuView.MenuButtonPrefab;
			view._generalButtonPrefab = menuView.GeneralButtonPrefab;
			view.ShowAtStart = false;
			view.Concurrent = false;
			view.AutoCloseChildren = true;
			view.HideOwner = false;
			view.IsBlockingInteration = true;
			view.CacheTypography(menuView.GetComponentInChildren<TextMeshProUGUI>(true));
			view.BuildShell();
			view.Initialize();
			view.gameObject.SetActive(false);
			UIManager.StaticViews[nameof(SoloAdventuresView)] = view;
			return view;
		}

		protected override void OnInit()
		{
			Instance = this;
		}

		protected override void OnShown()
		{
			RefreshGauntletUi();
			EnsureChampionVisualsLoadedAsync().Forget(Debug.LogException);
		}

		private void OnDestroy()
		{
			if (Instance == this)
				Instance = null;
		}

		private void CacheTypography(TMP_Text source)
		{
			if (!source)
				return;

			_fontAsset = source.font;
			_fontMaterial = source.fontSharedMaterial;
		}

		private void BuildShell()
		{
			var rootRect = RectTransform;
			rootRect.anchorMin = Vector2.zero;
			rootRect.anchorMax = Vector2.one;
			rootRect.offsetMin = Vector2.zero;
			rootRect.offsetMax = Vector2.zero;

			var dimmer = GetComponent<Image>();
			dimmer.color = new Color(0f, 0f, 0f, 0.72f);
			dimmer.raycastTarget = true;

			_panelRoot = CreateRect(
				"PanelRoot",
				transform,
				new Vector2(0.5f, 0.5f),
				new Vector2(0.5f, 0.5f),
				new Vector2(0.5f, 0.5f),
				Vector2.zero,
				new Vector2(860f, 620f));
			_panelBackground = _panelRoot.gameObject.AddComponent<Image>();
			_panelBackground.color = Color.white;
			_panelBackground.type = Image.Type.Simple;
			_panelBackground.preserveAspect = false;

			var panelShade = CreateFillImage("PanelShade", _panelRoot, new Color(0.04f, 0.04f, 0.05f, 0.36f));
			var panelBorder = _panelRoot.gameObject.AddComponent<Outline>();
			panelBorder.effectColor = new Color(0.92f, 0.72f, 0.25f, 0.88f);
			panelBorder.effectDistance = new Vector2(1.2f, -1.2f);
			panelBorder.useGraphicAlpha = true;
			panelShade.transform.SetAsLastSibling();

			_titleLabel = CreateText(
				"Title",
				_panelRoot,
				new Vector2(0.5f, 1f),
				new Vector2(0.5f, 1f),
				new Vector2(0.5f, 1f),
				new Vector2(0f, -10f),
				new Vector2(520f, 48f),
				34f,
				"SOLO ADVENTURES");
			_titleLabel.color = new Color(0.98f, 0.84f, 0.38f, 1f);
			_titleLabel.fontStyle = FontStyles.Bold;
			_titleLabel.alignment = TextAlignmentOptions.Center;

			_mainModesRoot = CreateRect(
				"MainModesRoot",
				_panelRoot,
				new Vector2(0.5f, 0.5f),
				new Vector2(0.5f, 0.5f),
				new Vector2(0.5f, 0.5f),
				new Vector2(0f, 8f),
				new Vector2(720f, 450f));

			_gauntletHeaderLabel = CreateText(
				"GauntletHeader",
				_mainModesRoot,
				new Vector2(0.5f, 1f),
				new Vector2(0.5f, 1f),
				new Vector2(0.5f, 1f),
				new Vector2(0f, -6f),
				new Vector2(620f, 26f),
				18f,
				"Choose a solo lane, then climb the five champion gauntlet.");
			_gauntletHeaderLabel.alignment = TextAlignmentOptions.Center;
			_gauntletHeaderLabel.color = new Color(0.9f, 0.9f, 0.9f, 0.92f);

			var cardsRow = CreateLayoutRow(
				"CardsRow",
				_mainModesRoot,
				new Vector2(0.5f, 0.5f),
				new Vector2(0.5f, 0.5f),
				new Vector2(0.5f, 0.5f),
				new Vector2(0f, -8f),
				new Vector2(640f, 410f),
				40f);

			CreateModeCard(
				cardsRow.transform,
				"CampaignCard",
				CampaignCardResource,
				"CAMPAIGN",
				"The Quadrant Trials",
				"0/129 Stars",
				() => Debug.Log("[SoloAdventuresView] Campaign clicked - not implemented yet"));

			_gauntletProgressLabel = CreateModeCard(
				cardsRow.transform,
				"GauntletCard",
				GauntletCardResource,
				"GAUNTLET",
				"Fight 5 Champions",
				GauntletProgressSettings.GetMainCardProgressText(),
				ShowGauntletSelection);

			_gauntletStarLabel = CreateText(
				"GauntletStars",
				_mainModesRoot,
				new Vector2(0.5f, 0f),
				new Vector2(0.5f, 0f),
				new Vector2(0.5f, 0f),
				new Vector2(0f, 42f),
				new Vector2(280f, 30f),
				24f,
				GauntletProgressSettings.GetStarTrackText());
			_gauntletStarLabel.alignment = TextAlignmentOptions.Center;
			_gauntletStarLabel.color = new Color(1f, 0.82f, 0.34f, 1f);
			_gauntletStarLabel.fontStyle = FontStyles.Bold;

			_gauntletSelectionRoot = CreateRect(
				"GauntletSelectionRoot",
				_panelRoot,
				new Vector2(0.5f, 0.5f),
				new Vector2(0.5f, 0.5f),
				new Vector2(0.5f, 0.5f),
				new Vector2(0f, 40f),
				new Vector2(620f, 470f));
			BuildGauntletSelection();
			_gauntletSelectionRoot.gameObject.SetActive(false);

			CreateBackButton();
		}

		public void ApplyMenuBackground(Sprite backgroundSprite)
		{
			if (!_panelBackground)
				return;

			_panelBackground.sprite = backgroundSprite;
			_panelBackground.color = backgroundSprite ? Color.white : new Color(0.06f, 0.06f, 0.07f, 0.92f);
		}

		public void ShowMainModesView()
		{
			ShowMainModes();
		}

		public void ShowGauntletSelectionView()
		{
			ShowGauntletSelection();
		}

		private void CreateBackButton()
		{
			var buttonGo = Instantiate(_generalButtonPrefab, _panelRoot, false);
			buttonGo.name = "BackButton";

			var rect = buttonGo.GetComponent<RectTransform>();
			rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
			rect.pivot = new Vector2(0.5f, 0f);
			rect.anchoredPosition = new Vector2(0f, 28f);
			rect.sizeDelta = new Vector2(300f, 72f);

			var button = buttonGo.GetComponent<Button>();
			button.onClick.AddListener(HandleBackPressed);

			_backButtonLabel = buttonGo.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
			if (_backButtonLabel != null)
				_backButtonLabel.text = "BACK";
		}

		private TextMeshProUGUI CreateModeCard(
			Transform parent,
			string objectName,
			string artResource,
			string title,
			string subtitle,
			string progress,
			Action onClick)
		{
			var cardGo = new GameObject(
				objectName,
				typeof(RectTransform),
				typeof(CanvasRenderer),
				typeof(Image),
				typeof(Button),
				typeof(LayoutElement));
			cardGo.transform.SetParent(parent, false);

			var layout = cardGo.GetComponent<LayoutElement>();
			layout.preferredWidth = 300f;
			layout.preferredHeight = 400f;

			var rect = cardGo.GetComponent<RectTransform>();
			rect.sizeDelta = new Vector2(300f, 400f);

			var cardBg = cardGo.GetComponent<Image>();
			cardBg.color = new Color(0.05f, 0.05f, 0.05f, 0.66f);

			var border = cardGo.AddComponent<Outline>();
			border.effectColor = new Color(0.92f, 0.72f, 0.25f, 0.78f);
			border.effectDistance = new Vector2(1.1f, -1.1f);
			border.useGraphicAlpha = true;

			var glow = CreateFillImage("Glow", cardGo.transform, new Color(1f, 0.48f, 0.08f, 0.12f));
			glow.enabled = false;

			var artFrame = CreateRect(
				"ArtFrame",
				cardGo.transform,
				new Vector2(0.5f, 1f),
				new Vector2(0.5f, 1f),
				new Vector2(0.5f, 1f),
				new Vector2(0f, -16f),
				new Vector2(260f, 240f));
			var artBg = artFrame.gameObject.AddComponent<Image>();
			artBg.color = new Color(0f, 0f, 0f, 0.55f);

			var art = new GameObject("Art", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
			art.transform.SetParent(artFrame, false);
			var artRect = art.GetComponent<RectTransform>();
			artRect.anchorMin = artRect.anchorMax = new Vector2(0.5f, 0.5f);
			artRect.pivot = new Vector2(0.5f, 0.5f);
			artRect.sizeDelta = new Vector2(252f, 232f);

			var rawImage = art.GetComponent<RawImage>();
			rawImage.texture = Resources.Load<Texture2D>(artResource);
			rawImage.color = Color.white;

			var titleLabel = CreateText(
				"ModeName",
				cardGo.transform,
				new Vector2(0.5f, 1f),
				new Vector2(0.5f, 1f),
				new Vector2(0.5f, 1f),
				new Vector2(0f, -278f),
				new Vector2(240f, 34f),
				20f,
				title);
			titleLabel.fontStyle = FontStyles.Bold;
			titleLabel.alignment = TextAlignmentOptions.Center;
			titleLabel.color = new Color(0.98f, 0.84f, 0.38f, 1f);

			var subtitleLabel = CreateText(
				"Subtitle",
				cardGo.transform,
				new Vector2(0.5f, 1f),
				new Vector2(0.5f, 1f),
				new Vector2(0.5f, 1f),
				new Vector2(0f, -316f),
				new Vector2(240f, 28f),
				15f,
				subtitle);
			subtitleLabel.alignment = TextAlignmentOptions.Center;
			subtitleLabel.color = new Color(0.88f, 0.88f, 0.88f, 0.94f);

			var progressLabel = CreateText(
				"Progress",
				cardGo.transform,
				new Vector2(0.5f, 0f),
				new Vector2(0.5f, 0f),
				new Vector2(0.5f, 0f),
				new Vector2(0f, 28f),
				new Vector2(240f, 30f),
				17f,
				progress);
			progressLabel.alignment = TextAlignmentOptions.Center;
			progressLabel.color = new Color(0.98f, 0.84f, 0.38f, 1f);
			progressLabel.fontStyle = FontStyles.Bold;

			var button = cardGo.GetComponent<Button>();
			var colors = button.colors;
			colors.normalColor = Color.white;
			colors.highlightedColor = new Color(1f, 0.92f, 0.84f, 1f);
			colors.pressedColor = new Color(0.82f, 0.74f, 0.66f, 1f);
			button.colors = colors;
			button.targetGraphic = cardBg;
			button.onClick.AddListener(() => onClick?.Invoke());

			var trigger = cardGo.AddComponent<EventTrigger>();
			AddPointerEvent(trigger, EventTriggerType.PointerEnter, () => glow.enabled = true);
			AddPointerEvent(trigger, EventTriggerType.PointerExit, () => glow.enabled = false);
			return progressLabel;
		}

		private void BuildGauntletSelection()
		{
			BuildGauntletProgressTrack();

			_gauntletCompletionBanner = CreateText(
				"CompletionBanner",
				_gauntletSelectionRoot,
				new Vector2(0.5f, 1f),
				new Vector2(0.5f, 1f),
				new Vector2(0.5f, 1f),
				new Vector2(0f, -85f),
				new Vector2(560f, 28f),
				18f,
				"FULL CLEAR ACHIEVED");
			_gauntletCompletionBanner.alignment = TextAlignmentOptions.Center;
			_gauntletCompletionBanner.color = new Color(0.76f, 1f, 0.73f, 1f);
			_gauntletCompletionBanner.fontStyle = FontStyles.Bold;
			_gauntletCompletionBanner.gameObject.SetActive(false);

			_gauntletIntroLabel = CreateText(
				"GauntletIntro",
				_gauntletSelectionRoot,
				new Vector2(0.5f, 1f),
				new Vector2(0.5f, 1f),
				new Vector2(0.5f, 1f),
				new Vector2(0f, -85f),
				new Vector2(560f, 26f),
				18f,
				"Choose a champion to challenge");
			_gauntletIntroLabel.alignment = TextAlignmentOptions.Center;
			_gauntletIntroLabel.color = new Color(0.9f, 0.9f, 0.9f, 0.92f);

			var introRule = CreateFillImage("IntroRule", _gauntletSelectionRoot, new Color(0.92f, 0.72f, 0.25f, 0.32f));
			var introRuleRect = introRule.rectTransform;
			introRuleRect.anchorMin = introRuleRect.anchorMax = new Vector2(0.5f, 1f);
			introRuleRect.pivot = new Vector2(0.5f, 0.5f);
			introRuleRect.anchoredPosition = new Vector2(0f, -126f);
			introRuleRect.sizeDelta = new Vector2(480f, 2f);

			var list = new GameObject("GauntletList", typeof(RectTransform), typeof(VerticalLayoutGroup));
			list.transform.SetParent(_gauntletSelectionRoot, false);
			var listRect = list.GetComponent<RectTransform>();
			listRect.anchorMin = listRect.anchorMax = new Vector2(0.5f, 0.5f);
			listRect.pivot = new Vector2(0.5f, 0.5f);
			listRect.anchoredPosition = new Vector2(0f, -16f);
			listRect.sizeDelta = new Vector2(560f, 288f);

			var layout = list.GetComponent<VerticalLayoutGroup>();
			layout.childAlignment = TextAnchor.UpperCenter;
			layout.spacing = 8f;
			layout.childControlWidth = false;
			layout.childControlHeight = false;
			layout.childForceExpandWidth = false;
			layout.childForceExpandHeight = false;

			CreateChampionButton(list.transform, "KaelBtn", "KAEL THE INITIATE", "Opening Trial", "bot_gauntlet_1");
			CreateChampionButton(list.transform, "LyraBtn", "LYRA THORNWEAVER", "Thorns and Tempo", "bot_gauntlet_2");
			CreateChampionButton(list.transform, "DravenBtn", "DRAVEN ASHBORN", "Pressure and Removal", "bot_gauntlet_3");
			CreateChampionButton(list.transform, "SerisBtn", "SERIS TIDECALLER", "Hard+ Controller", "bot_gauntlet_4");
			CreateChampionButton(list.transform, "KronosBtn", "KRONOS THE UNBROKEN", "Brutal Final Boss", "bot_gauntlet_5");
		}

		private void BuildGauntletProgressTrack()
		{
			var trackRoot = CreateRect(
				"GauntletTrackRoot",
				_gauntletSelectionRoot,
				new Vector2(0.5f, 1f),
				new Vector2(0.5f, 1f),
				new Vector2(0.5f, 1f),
				new Vector2(0f, -20f),
				new Vector2(560f, 44f));

			for (var i = 0; i < GauntletProgressSettings.ChampionCount; i++)
			{
				var slot = CreateRect(
					$"TrackNode_{i}",
					trackRoot,
					new Vector2(0.5f, 0.5f),
					new Vector2(0.5f, 0.5f),
					new Vector2(0.5f, 0.5f),
					new Vector2(-224f + i * 112f, 0f),
					new Vector2(44f, 44f));

				var fill = slot.gameObject.AddComponent<Image>();
				fill.color = new Color(0.18f, 0.18f, 0.2f, 0.96f);

				var outline = slot.gameObject.AddComponent<Outline>();
				outline.effectColor = new Color(0.92f, 0.72f, 0.25f, 0.72f);
				outline.effectDistance = new Vector2(1f, -1f);
				outline.useGraphicAlpha = true;

				var label = CreateText(
					"Label",
					slot,
					new Vector2(0.5f, 0.5f),
					new Vector2(0.5f, 0.5f),
					new Vector2(0.5f, 0.5f),
					Vector2.zero,
					new Vector2(32f, 24f),
					18f,
					(i + 1).ToString());
				label.alignment = TextAlignmentOptions.Center;
				label.fontStyle = FontStyles.Bold;

				var caption = CreateText(
					"Caption",
					slot,
					new Vector2(0.5f, 0f),
					new Vector2(0.5f, 0f),
					new Vector2(0.5f, 1f),
					new Vector2(0f, -8f),
					new Vector2(72f, 16f),
					11f,
					"STAR");
				caption.alignment = TextAlignmentOptions.Center;
				caption.color = new Color(0.82f, 0.82f, 0.82f, 0.85f);

				Image connector = null;
				if (i < GauntletProgressSettings.ChampionCount - 1)
				{
					connector = CreateFillImage($"Connector_{i}", trackRoot, new Color(0.44f, 0.34f, 0.18f, 0.6f));
					var connectorRect = connector.rectTransform;
					connectorRect.anchorMin = connectorRect.anchorMax = new Vector2(0.5f, 0.5f);
					connectorRect.pivot = new Vector2(0.5f, 0.5f);
					connectorRect.anchoredPosition = new Vector2(-168f + i * 112f, 0f);
					connectorRect.sizeDelta = new Vector2(68f, 3f);
				}

				_trackerNodes.Add(new TrackerNodeState
				{
					Fill = fill,
					Label = label,
					Caption = caption,
					Connector = connector
				});
			}
		}

		private void CreateChampionButton(Transform parent, string objectName, string title, string subtitle, string botDeckId)
		{
			var buttonGo = new GameObject(
				objectName,
				typeof(RectTransform),
				typeof(CanvasRenderer),
				typeof(Image),
				typeof(Button),
				typeof(LayoutElement));
			buttonGo.transform.SetParent(parent, false);

			var layout = buttonGo.GetComponent<LayoutElement>();
			layout.preferredWidth = 520f;
			layout.preferredHeight = 68f;

			var rect = buttonGo.GetComponent<RectTransform>();
			rect.sizeDelta = new Vector2(520f, 68f);

			var image = buttonGo.GetComponent<Image>();
			image.sprite = Resources.Load<Sprite>(ButtonBackgroundResource);
			image.type = Image.Type.Sliced;
			image.color = new Color(0.95f, 0.95f, 0.95f, 1f);

			var themeStripe = CreateFillImage("ThemeStripe", buttonGo.transform, GetChampionThemeColor(botDeckId));
			themeStripe.rectTransform.anchorMin = new Vector2(0f, 0f);
			themeStripe.rectTransform.anchorMax = new Vector2(0f, 1f);
			themeStripe.rectTransform.pivot = new Vector2(0f, 0.5f);
			themeStripe.rectTransform.offsetMin = new Vector2(0f, 0f);
			themeStripe.rectTransform.offsetMax = new Vector2(10f, 0f);

			var accentBar = CreateFillImage("AccentBar", buttonGo.transform, new Color(1f, 0.5f, 0.08f, 0.9f));
			accentBar.rectTransform.anchorMin = new Vector2(0f, 1f);
			accentBar.rectTransform.anchorMax = new Vector2(1f, 1f);
			accentBar.rectTransform.pivot = new Vector2(0.5f, 1f);
			accentBar.rectTransform.offsetMin = new Vector2(16f, -4f);
			accentBar.rectTransform.offsetMax = new Vector2(-16f, 0f);
			accentBar.enabled = false;

			var button = buttonGo.GetComponent<Button>();
			var colors = button.colors;
			colors.normalColor = Color.white;
			colors.highlightedColor = new Color(1f, 0.95f, 0.84f, 1f);
			colors.pressedColor = new Color(0.86f, 0.79f, 0.7f, 1f);
			button.colors = colors;
			button.targetGraphic = image;
			button.onClick.AddListener(() => LaunchGauntletAsync(botDeckId).AddLoadingTask().Forget(Debug.LogException));

			var lockShade = CreateFillImage("LockShade", buttonGo.transform, new Color(0f, 0f, 0f, 0.34f));
			lockShade.enabled = false;

			var tierBadgeGo = new GameObject("TierBadge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
			tierBadgeGo.transform.SetParent(buttonGo.transform, false);
			var tierBadge = tierBadgeGo.GetComponent<Image>();
			tierBadge.color = new Color(0.22f, 0.18f, 0.12f, 0.9f);

			var tierBadgeRect = tierBadge.rectTransform;
			tierBadgeRect.anchorMin = tierBadgeRect.anchorMax = new Vector2(0f, 0.5f);
			tierBadgeRect.pivot = new Vector2(0.5f, 0.5f);
			tierBadgeRect.anchoredPosition = new Vector2(42f, 0f);
			tierBadgeRect.sizeDelta = new Vector2(38f, 38f);

			var tierOutline = tierBadgeGo.AddComponent<Outline>();
			tierOutline.effectColor = new Color(0.92f, 0.72f, 0.25f, 0.66f);
			tierOutline.effectDistance = new Vector2(1f, -1f);
			tierOutline.useGraphicAlpha = true;

			var tierLabel = CreateText(
				"TierLabel",
				tierBadgeGo.transform,
				new Vector2(0.5f, 0.5f),
				new Vector2(0.5f, 0.5f),
				new Vector2(0.5f, 0.5f),
				Vector2.zero,
				new Vector2(28f, 24f),
				17f,
				(_championButtons.Count + 1).ToString());
			tierLabel.alignment = TextAlignmentOptions.Center;
			tierLabel.fontStyle = FontStyles.Bold;
			tierLabel.color = new Color(1f, 0.82f, 0.34f, 1f);

			var portraitRoot = CreateRect(
				"PortraitRoot",
				buttonGo.transform,
				new Vector2(0f, 0.5f),
				new Vector2(0f, 0.5f),
				new Vector2(0.5f, 0.5f),
				new Vector2(100f, 0f),
				new Vector2(50f, 50f));

			var portraitBackdrop = portraitRoot.gameObject.AddComponent<Image>();
			portraitBackdrop.color = new Color(0.08f, 0.08f, 0.1f, 0.72f);

			var portraitOutline = portraitRoot.gameObject.AddComponent<Outline>();
			portraitOutline.effectColor = new Color(0.92f, 0.72f, 0.25f, 0.52f);
			portraitOutline.effectDistance = new Vector2(1f, -1f);
			portraitOutline.useGraphicAlpha = true;

			var portraitGo = new GameObject("Portrait", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
			portraitGo.transform.SetParent(portraitRoot, false);
			var portraitRect = portraitGo.GetComponent<RectTransform>();
			portraitRect.anchorMin = portraitRect.anchorMax = new Vector2(0.5f, 0.5f);
			portraitRect.pivot = new Vector2(0.5f, 0.5f);
			portraitRect.sizeDelta = new Vector2(46f, 46f);

			var portraitImage = portraitGo.GetComponent<RawImage>();
			portraitImage.color = Color.white;

			var portraitFrameGo = new GameObject("PortraitFrame", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
			portraitFrameGo.transform.SetParent(portraitRoot, false);
			var portraitFrameRect = portraitFrameGo.GetComponent<RectTransform>();
			portraitFrameRect.anchorMin = portraitFrameRect.anchorMax = new Vector2(0.5f, 0.5f);
			portraitFrameRect.pivot = new Vector2(0.5f, 0.5f);
			portraitFrameRect.sizeDelta = new Vector2(56f, 56f);

			var portraitFrameImage = portraitFrameGo.GetComponent<RawImage>();
			portraitFrameImage.color = new Color(1f, 1f, 1f, 0.96f);

			var titleLabel = CreateText(
				"Title",
				buttonGo.transform,
				new Vector2(0f, 0.5f),
				new Vector2(0f, 0.5f),
				new Vector2(0f, 0.5f),
				new Vector2(142f, 14f),
				new Vector2(220f, 22f),
				18f,
				title);
			titleLabel.alignment = TextAlignmentOptions.MidlineLeft;
			titleLabel.fontStyle = FontStyles.Bold;
			titleLabel.color = new Color(0.98f, 0.84f, 0.38f, 1f);

			var subtitleLabel = CreateText(
				"Subtitle",
				buttonGo.transform,
				new Vector2(0f, 0.5f),
				new Vector2(0f, 0.5f),
				new Vector2(0f, 0.5f),
				new Vector2(142f, -4f),
				new Vector2(220f, 18f),
				12f,
				subtitle);
			subtitleLabel.alignment = TextAlignmentOptions.MidlineLeft;
			subtitleLabel.color = new Color(0.9f, 0.9f, 0.9f, 0.94f);

			var metaLabel = CreateText(
				"Meta",
				buttonGo.transform,
				new Vector2(0f, 0.5f),
				new Vector2(0f, 0.5f),
				new Vector2(0f, 0.5f),
				new Vector2(142f, -20f),
				new Vector2(220f, 18f),
				10f,
				GetChampionMeta(botDeckId));
			metaLabel.alignment = TextAlignmentOptions.MidlineLeft;
			metaLabel.color = new Color(0.8f, 0.8f, 0.8f, 0.92f);

			var rewardLabel = CreateText(
				"Reward",
				buttonGo.transform,
				new Vector2(1f, 0.5f),
				new Vector2(1f, 0.5f),
				new Vector2(1f, 0.5f),
				new Vector2(-154f, -18f),
				new Vector2(150f, 18f),
				10f,
				GetChampionReward(botDeckId));
			rewardLabel.alignment = TextAlignmentOptions.MidlineRight;
			rewardLabel.color = new Color(1f, 0.82f, 0.34f, 0.95f);
			rewardLabel.fontStyle = FontStyles.Bold;

			var statusChipGo = new GameObject("StatusChip", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
			statusChipGo.transform.SetParent(buttonGo.transform, false);
			var statusChip = statusChipGo.GetComponent<Image>();
			statusChip.color = new Color(0.26f, 0.19f, 0.1f, 0.9f);

			var statusChipRect = statusChip.rectTransform;
			statusChipRect.anchorMin = statusChipRect.anchorMax = new Vector2(1f, 0.5f);
			statusChipRect.pivot = new Vector2(1f, 0.5f);
			statusChipRect.anchoredPosition = new Vector2(-22f, 0f);
			statusChipRect.sizeDelta = new Vector2(118f, 28f);

			var statusOutline = statusChipGo.AddComponent<Outline>();
			statusOutline.effectColor = new Color(0.92f, 0.72f, 0.25f, 0.58f);
			statusOutline.effectDistance = new Vector2(1f, -1f);
			statusOutline.useGraphicAlpha = true;

			var statusLabel = CreateText(
				"Status",
				statusChipGo.transform,
				new Vector2(0.5f, 0.5f),
				new Vector2(0.5f, 0.5f),
				new Vector2(0.5f, 0.5f),
				Vector2.zero,
				new Vector2(100f, 22f),
				16f,
				"READY");
			statusLabel.alignment = TextAlignmentOptions.Center;
			statusLabel.fontStyle = FontStyles.Bold;

			_championButtons.Add(new ChampionButtonState
			{
				BotDeckId = botDeckId,
				Title = title,
				Button = button,
				Background = image,
				AccentBar = accentBar,
				TierBadge = tierBadge,
				TierLabel = tierLabel,
				TitleLabel = titleLabel,
				SubtitleLabel = subtitleLabel,
				MetaLabel = metaLabel,
				RewardLabel = rewardLabel,
				StatusChip = statusChip,
				StatusLabel = statusLabel,
				LockShade = lockShade,
				ThemeStripe = themeStripe,
				PortraitImage = portraitImage,
				PortraitFrameImage = portraitFrameImage
			});
		}

		private void ShowGauntletSelection()
		{
			if (_mainModesRoot)
				_mainModesRoot.gameObject.SetActive(false);
			if (_gauntletSelectionRoot)
				_gauntletSelectionRoot.gameObject.SetActive(true);
			if (_titleLabel)
				_titleLabel.text = "GAUNTLET";
			if (_backButtonLabel)
				_backButtonLabel.text = "BACK TO SOLO";

			RefreshGauntletUi();
		}

		private void ShowMainModes()
		{
			if (_mainModesRoot)
				_mainModesRoot.gameObject.SetActive(true);
			if (_gauntletSelectionRoot)
				_gauntletSelectionRoot.gameObject.SetActive(false);
			if (_titleLabel)
				_titleLabel.text = "SOLO ADVENTURES";
			if (_backButtonLabel)
				_backButtonLabel.text = "BACK";

			RefreshGauntletUi();
		}

		private void HandleBackPressed()
		{
			if (_gauntletSelectionRoot != null && _gauntletSelectionRoot.gameObject.activeSelf)
			{
				ShowMainModes();
				return;
			}

			ShowMainModes();
			Close();
		}

		private async UniTask LaunchGauntletAsync(string botDeckId)
		{
			if (GauntletProgressSettings.TryGetChampionIndex(botDeckId, out var championIndex)
			    && !GauntletProgressSettings.IsUnlocked(championIndex))
			{
				UIManager.ShowWarningDialog("Defeat the current champion to unlock this battle.");
				return;
			}

			var selectedDeckId = DeckApplicationAdapter.Application?.Current?.Id;
			if (string.IsNullOrWhiteSpace(selectedDeckId))
			{
				UIManager.ShowWarningDialog("Select a deck before starting Gauntlet.");
				return;
			}

			var response = await LobbyAPI.PostPracticeStartSession(new LobbyPracticeStartSessionModel
			{
				MatchMode = MatchMode.Practice,
				Difficulty = PracticeMode.Hard,
				DeckId = selectedDeckId,
				BotDeckId = botDeckId
			});

			if (!response)
			{
				UIManager.ShowWarningDialog(response.GetMessage());
				return;
			}

			if (PracticeApplicationAdapter.Application == null || response.Data == null)
			{
				UIManager.ShowWarningDialog("Gauntlet session started, but the client could not join it.");
				return;
			}

				var gauntletArenaTheme = GetGauntletArenaTheme(botDeckId);
				GauntletMatchPresentation.Activate(gauntletArenaTheme);
				ArenaThemeSettings.SetRuntimeOverride(gauntletArenaTheme);
				BoardLayoutSettings.SetRuntimeOverride(true);

			Close();

			if (!await PracticeApplicationAdapter.Application.JoinGameAsync(response.Data))
				UIManager.ShowWarningDialog("Gauntlet session started, but the client could not join it.");
		}

		private static string GetGauntletArenaTheme(string botDeckId)
		{
			return botDeckId switch
			{
				"bot_gauntlet_1" => ArenaThemeSettings.Boreas,
				"bot_gauntlet_2" => ArenaThemeSettings.Arcadia,
				"bot_gauntlet_3" => ArenaThemeSettings.Hades,
				"bot_gauntlet_4" => ArenaThemeSettings.Notus,
				"bot_gauntlet_5" => ArenaThemeSettings.Colosseum,
				_ => ArenaThemeSettings.GetSelectedValue()
			};
		}

		private void RefreshGauntletUi()
		{
			if (_gauntletProgressLabel)
				_gauntletProgressLabel.text = GauntletProgressSettings.GetMainCardProgressText();
			if (_gauntletStarLabel)
				_gauntletStarLabel.text = GauntletProgressSettings.GetStarTrackText();
			if (_gauntletHeaderLabel)
			{
				_gauntletHeaderLabel.text = GauntletProgressSettings.IsFullyCleared()
					? "All five champions are down. Replay any fight to pressure-test new decks."
					: "Clear each champion in order to unlock the next arena and earn another star.";
			}
			if (_gauntletCompletionBanner)
				_gauntletCompletionBanner.gameObject.SetActive(GauntletProgressSettings.IsFullyCleared());

			if (_gauntletIntroLabel)
			{
				if (GauntletProgressSettings.IsFullyCleared())
				{
					_gauntletIntroLabel.text = "All champions cleared. Replay any tier to keep testing your decks.";
				}
				else
				{
					var nextIndex = GauntletProgressSettings.GetNextChampionIndex();
					var nextTitle = nextIndex >= 0 && nextIndex < _championButtons.Count
						? _championButtons[nextIndex].Title
						: "the next champion";
					_gauntletIntroLabel.text = $"Defeat {nextTitle} to unlock the next trial.";
				}
			}

			foreach (var champion in _championButtons)
			{
				if (!GauntletProgressSettings.TryGetChampionIndex(champion.BotDeckId, out var index))
					continue;

				var isCleared = GauntletProgressSettings.IsCleared(index);
				var isUnlocked = GauntletProgressSettings.IsUnlocked(index);
				var isCurrent = !isCleared && isUnlocked;

				champion.Button.interactable = isUnlocked;
				champion.Background.color = isUnlocked ? Color.white : new Color(0.62f, 0.62f, 0.62f, 0.9f);
				champion.AccentBar.enabled = isCurrent;
				champion.LockShade.enabled = !isUnlocked;
				champion.ThemeStripe.color = isUnlocked
					? GetChampionThemeColor(champion.BotDeckId)
					: new Color(0.3f, 0.3f, 0.3f, 0.92f);
				champion.TierBadge.color = isCleared
					? new Color(0.16f, 0.36f, 0.2f, 0.95f)
					: isCurrent
						? new Color(0.34f, 0.2f, 0.08f, 0.96f)
						: new Color(0.22f, 0.18f, 0.12f, 0.9f);
				champion.TierLabel.color = isUnlocked
					? new Color(1f, 0.82f, 0.34f, 1f)
					: new Color(0.78f, 0.78f, 0.78f, 1f);
				champion.TitleLabel.color = isCleared
					? new Color(0.76f, 1f, 0.73f, 1f)
					: new Color(0.98f, 0.84f, 0.38f, 1f);
				champion.SubtitleLabel.color = isUnlocked
					? new Color(0.9f, 0.9f, 0.9f, 0.94f)
					: new Color(0.74f, 0.74f, 0.74f, 0.9f);
				champion.MetaLabel.color = isUnlocked
					? new Color(0.78f, 0.78f, 0.78f, 0.92f)
					: new Color(0.58f, 0.58f, 0.58f, 0.88f);
				champion.RewardLabel.color = isCleared
					? new Color(0.76f, 1f, 0.73f, 0.98f)
					: isUnlocked
						? new Color(1f, 0.82f, 0.34f, 0.95f)
						: new Color(0.66f, 0.66f, 0.66f, 0.9f);
				champion.MetaLabel.text = GetChampionMeta(champion.BotDeckId);
				champion.RewardLabel.text = isCleared ? "★ STAR SECURED" : GetChampionReward(champion.BotDeckId);
				if (champion.PortraitFrameImage)
				{
					champion.PortraitFrameImage.color = isUnlocked
						? new Color(1f, 1f, 1f, 0.98f)
						: new Color(0.64f, 0.64f, 0.64f, 0.9f);
				}

				if (isCleared)
				{
					champion.StatusLabel.text = "CLEARED";
					champion.StatusChip.color = new Color(0.16f, 0.36f, 0.2f, 0.92f);
					champion.StatusLabel.color = new Color(0.76f, 1f, 0.73f, 1f);
				}
				else if (isCurrent)
				{
					champion.StatusLabel.text = "READY";
					champion.StatusChip.color = new Color(0.36f, 0.2f, 0.08f, 0.92f);
					champion.StatusLabel.color = new Color(1f, 0.78f, 0.28f, 1f);
				}
				else
				{
					champion.StatusLabel.text = "LOCKED";
					champion.StatusChip.color = new Color(0.24f, 0.24f, 0.24f, 0.92f);
					champion.StatusLabel.color = new Color(0.78f, 0.78f, 0.78f, 0.96f);
				}
			}

			var completedCount = GauntletProgressSettings.GetCompletedCount();
			for (var i = 0; i < _trackerNodes.Count; i++)
			{
				var node = _trackerNodes[i];
				var isCleared = i < completedCount;
				var isCurrent = i == Mathf.Min(completedCount, _trackerNodes.Count - 1) && completedCount < _trackerNodes.Count;

				node.Fill.color = isCleared
					? new Color(0.16f, 0.36f, 0.2f, 0.96f)
					: isCurrent
						? new Color(0.42f, 0.24f, 0.08f, 0.98f)
						: new Color(0.18f, 0.18f, 0.2f, 0.96f);
				node.Label.color = isCleared
					? new Color(0.76f, 1f, 0.73f, 1f)
					: isCurrent
						? new Color(1f, 0.82f, 0.34f, 1f)
						: new Color(0.78f, 0.78f, 0.78f, 0.96f);
				if (node.Caption)
				{
					node.Caption.text = isCleared ? "SECURED" : isCurrent ? "CURRENT" : "LOCKED";
					node.Caption.color = isCleared
						? new Color(0.76f, 1f, 0.73f, 0.96f)
						: isCurrent
							? new Color(1f, 0.82f, 0.34f, 0.96f)
							: new Color(0.72f, 0.72f, 0.72f, 0.9f);
				}

				if (node.Connector != null)
				{
					node.Connector.color = i < completedCount - 1
						? new Color(0.76f, 1f, 0.73f, 0.66f)
						: new Color(0.44f, 0.34f, 0.18f, 0.6f);
				}
			}
		}

		private async UniTask EnsureChampionVisualsLoadedAsync()
		{
			if (_championVisualsLoaded)
				return;

			_championVisualsLoaded = true;
			var tasks = new List<UniTask>(_championButtons.Count * 2);
			foreach (var champion in _championButtons)
			{
				if (champion.PortraitImage)
					tasks.Add(champion.PortraitImage.LoadResourceAsync(GetChampionPortraitArt(champion.BotDeckId)));

				if (champion.PortraitFrameImage)
					tasks.Add(champion.PortraitFrameImage.LoadResourceAsync(GetChampionPortraitFrame(champion.BotDeckId)));
			}

			await UniTask.WhenAll(tasks);
		}

		private static string GetChampionMeta(string botDeckId)
		{
			return botDeckId switch
			{
				"bot_gauntlet_1" => "Boreas Arena  •  Normal Brain",
				"bot_gauntlet_2" => "Arcadia Arena  •  Normal+ Tempo",
				"bot_gauntlet_3" => "Hades Arena  •  Hard Pressure",
				"bot_gauntlet_4" => "Notus Arena  •  Hard+ Control",
				"bot_gauntlet_5" => "Colosseum  •  Brutal Boss",
				_ => "Custom Arena  •  Practice Battle"
			};
		}

		private static string GetChampionReward(string botDeckId)
		{
			return botDeckId switch
			{
				"bot_gauntlet_5" => "★ FINAL STAR",
				_ => "★ CLAIM STAR"
			};
		}

		private static string GetChampionPortraitArt(string botDeckId)
		{
			return botDeckId switch
			{
				"bot_gauntlet_1" => "SILVER WARRIOR_1674726718",
				"bot_gauntlet_2" => "Dreamweaver",
				"bot_gauntlet_3" => "DEATH_DEALER_1674726713",
				"bot_gauntlet_4" => "Mantichora",
				"bot_gauntlet_5" => "GensoKnight",
				_ => "SILVER WARRIOR_1674726718"
			};
		}

		private static string GetChampionPortraitFrame(string botDeckId)
		{
			return botDeckId switch
			{
				"bot_gauntlet_1" => "Vulcanite_Border_Boreas",
				"bot_gauntlet_2" => "Vulcanite_Border_Arcadia",
				"bot_gauntlet_3" => "Vulcanite_Border_Hades",
				"bot_gauntlet_4" => "Vulcanite_Border_Notus",
				"bot_gauntlet_5" => "Vulcanite_Border_Neutral",
				_ => "Vulcanite_Border_Neutral"
			};
		}

		private static Color GetChampionThemeColor(string botDeckId)
		{
			return botDeckId switch
			{
				"bot_gauntlet_1" => new Color(0.45f, 0.72f, 0.95f, 0.95f),
				"bot_gauntlet_2" => new Color(0.48f, 0.76f, 0.36f, 0.95f),
				"bot_gauntlet_3" => new Color(0.78f, 0.36f, 0.28f, 0.95f),
				"bot_gauntlet_4" => new Color(0.86f, 0.7f, 0.32f, 0.95f),
				"bot_gauntlet_5" => new Color(0.92f, 0.58f, 0.18f, 0.98f),
				_ => new Color(0.82f, 0.82f, 0.82f, 0.95f)
			};
		}

		private HorizontalLayoutGroup CreateLayoutRow(
			string name,
			Transform parent,
			Vector2 anchorMin,
			Vector2 anchorMax,
			Vector2 pivot,
			Vector2 anchoredPosition,
			Vector2 size,
			float spacing)
		{
			var row = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup));
			row.transform.SetParent(parent, false);

			var rect = row.GetComponent<RectTransform>();
			rect.anchorMin = anchorMin;
			rect.anchorMax = anchorMax;
			rect.pivot = pivot;
			rect.anchoredPosition = anchoredPosition;
			rect.sizeDelta = size;

			var layout = row.GetComponent<HorizontalLayoutGroup>();
			layout.childAlignment = TextAnchor.MiddleCenter;
			layout.spacing = spacing;
			layout.childControlWidth = false;
			layout.childControlHeight = false;
			layout.childForceExpandWidth = false;
			layout.childForceExpandHeight = false;
			return layout;
		}

		private Image CreateFillImage(string name, Transform parent, Color color)
		{
			var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
			go.transform.SetParent(parent, false);

			var rect = go.GetComponent<RectTransform>();
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.offsetMin = Vector2.zero;
			rect.offsetMax = Vector2.zero;

			var image = go.GetComponent<Image>();
			image.color = color;
			image.raycastTarget = false;
			return image;
		}

		private RectTransform CreateRect(
			string name,
			Transform parent,
			Vector2 anchorMin,
			Vector2 anchorMax,
			Vector2 pivot,
			Vector2 anchoredPosition,
			Vector2 size)
		{
			var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
			rect.SetParent(parent, false);
			rect.anchorMin = anchorMin;
			rect.anchorMax = anchorMax;
			rect.pivot = pivot;
			rect.anchoredPosition = anchoredPosition;
			rect.sizeDelta = size;
			return rect;
		}

		private TextMeshProUGUI CreateText(
			string name,
			Transform parent,
			Vector2 anchorMin,
			Vector2 anchorMax,
			Vector2 pivot,
			Vector2 anchoredPosition,
			Vector2 size,
			float fontSize,
			string value)
		{
			var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
			go.transform.SetParent(parent, false);

			var rect = go.GetComponent<RectTransform>();
			rect.anchorMin = anchorMin;
			rect.anchorMax = anchorMax;
			rect.pivot = pivot;
			rect.anchoredPosition = anchoredPosition;
			rect.sizeDelta = size;

			var text = go.GetComponent<TextMeshProUGUI>();
			if (_fontAsset)
				text.font = _fontAsset;
			if (_fontMaterial)
				text.fontSharedMaterial = _fontMaterial;
			text.fontSize = fontSize;
			text.text = value;
			text.enableWordWrapping = false;
			text.raycastTarget = false;
			return text;
		}

		private static void AddPointerEvent(EventTrigger trigger, EventTriggerType type, Action action)
		{
			var entry = new EventTrigger.Entry { eventID = type };
			entry.callback.AddListener(_ => action?.Invoke());
			trigger.triggers.Add(entry);
		}
	}
}
