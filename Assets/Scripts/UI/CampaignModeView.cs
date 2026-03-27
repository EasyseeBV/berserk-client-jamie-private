using System;
using TMPro;
using RR.UI.FrameSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
	public class CampaignModeView : BaseView
	{
		private const string CardFrameResource = "UI/button-frame";

		public static CampaignModeView Instance { get; private set; }

		private GameObject _generalButtonPrefab;
		private TMP_FontAsset _fontAsset;
		private Material _fontMaterial;
		private RectTransform _panelRoot;
		private Image _panelBackground;
		private TextMeshProUGUI _titleLabel;
		private TextMeshProUGUI _subtitleLabel;
		private TextMeshProUGUI _backLabel;
		private RectTransform _quadrantsRoot;
		private RectTransform _stageMapRoot;
		private CampaignQuadrantDefinition _currentQuadrant;
		private TextMeshProUGUI _stageDetailTitle;
		private TextMeshProUGUI _stageDetailBody;
		private TextMeshProUGUI _stageDetailState;
		private RawImage _stageDetailArt;

		public static CampaignModeView EnsureInstance(MenuView menuView)
		{
			if (Instance)
				return Instance;

			var parent = menuView.transform.parent as RectTransform;
			if (!parent)
				parent = menuView.GetComponentInParent<Canvas>()?.transform as RectTransform;

			var root = new GameObject(
				nameof(CampaignModeView),
				typeof(RectTransform),
				typeof(CanvasRenderer),
				typeof(Image),
				typeof(CanvasGroup),
				typeof(CampaignModeView));
			root.transform.SetParent(parent, false);

			var view = root.GetComponent<CampaignModeView>();
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
			UIManager.StaticViews[nameof(CampaignModeView)] = view;
			return view;
		}

		protected override void OnInit()
		{
			Instance = this;
		}

		private void OnDestroy()
		{
			if (Instance == this)
				Instance = null;
		}

		public void ApplyMenuBackground(Sprite backgroundSprite)
		{
			if (!_panelBackground)
				return;

			_panelBackground.sprite = backgroundSprite;
			_panelBackground.color = backgroundSprite ? Color.white : new Color(0.06f, 0.06f, 0.07f, 0.92f);
		}

		public void ShowQuadrantSelection()
		{
			_currentQuadrant = null;
			_titleLabel.text = "THE QUADRANT TRIALS";
			_subtitleLabel.text = "Choose a quadrant to begin your campaign run.";
			_quadrantsRoot.gameObject.SetActive(true);
			_stageMapRoot.gameObject.SetActive(false);
			_backLabel.text = "BACK TO SOLO";
		}

		private void ShowStageMap(CampaignQuadrantDefinition quadrant)
		{
			if (quadrant == null)
				return;

			_currentQuadrant = quadrant;
			_titleLabel.text = quadrant.DisplayName;
			_subtitleLabel.text = $"{quadrant.Subtitle}  Heroic Mode: Locked";
			_quadrantsRoot.gameObject.SetActive(false);
			_stageMapRoot.gameObject.SetActive(true);
			_backLabel.text = "BACK TO QUADRANTS";
			BuildStageMapContent();
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

			_panelRoot = CreateRect("PanelRoot", transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(960f, 620f));
			_panelBackground = _panelRoot.gameObject.AddComponent<Image>();
			var panelBorder = _panelRoot.gameObject.AddComponent<Outline>();
			panelBorder.effectColor = new Color(0.92f, 0.72f, 0.25f, 0.88f);
			panelBorder.effectDistance = new Vector2(1.2f, -1.2f);
			panelBorder.useGraphicAlpha = true;

			var headerBand = CreateRect("HeaderBand", _panelRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -10f), new Vector2(900f, 110f));
			var headerBandImage = headerBand.gameObject.AddComponent<Image>();
			headerBandImage.color = new Color(0.03f, 0.04f, 0.06f, 0.34f);
			headerBandImage.enabled = false;
			_titleLabel = CreateText("Title", _panelRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -10f), new Vector2(640f, 48f), 34f, "THE QUADRANT TRIALS");
			_titleLabel.color = new Color(0.98f, 0.84f, 0.38f, 1f);
			_titleLabel.fontStyle = FontStyles.Bold;
			_titleLabel.alignment = TextAlignmentOptions.Center;

			_subtitleLabel = CreateText("Subtitle", _panelRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(760f, 24f), 17f, "Choose a quadrant to begin your campaign run.");
			_subtitleLabel.alignment = TextAlignmentOptions.Center;
			_subtitleLabel.color = new Color(0.9f, 0.9f, 0.9f, 0.92f);

			_quadrantsRoot = CreateRect("QuadrantsRoot", _panelRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(860f, 490f));
			BuildQuadrantSelection();

			_stageMapRoot = CreateRect("StageMapRoot", _panelRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -2f), new Vector2(860f, 520f));
			_stageMapRoot.gameObject.SetActive(false);

			CreateBackButton();
		}

		private void BuildQuadrantSelection()
		{
			var layoutRoot = CreateRect("QuadrantGrid", _quadrantsRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -6f), new Vector2(840f, 490f));
			var backdrop = layoutRoot.gameObject.AddComponent<Image>();
			backdrop.color = new Color(0.03f, 0.04f, 0.05f, 0.18f);
			backdrop.enabled = false;
			for (var i = 0; i < CampaignModeSettings.Quadrants.Count; i++)
			{
				var quadrant = CampaignModeSettings.Quadrants[i];
				var isVulcanCity = quadrant.Id == "vulcan_city";
				var row = isVulcanCity ? 2 : i / 2;
				var column = isVulcanCity ? 0 : i % 2;
				var x = isVulcanCity ? 0f : (column == 0 ? -210f : 210f);
				var y = row switch
				{
					0 => 100f,
					1 => -72f,
					_ => -210f
				};
				var width = isVulcanCity ? 620f : 364f;
				var height = isVulcanCity ? 92f : 160f;
				CreateQuadrantCard(layoutRoot, quadrant, new Vector2(x, y), new Vector2(width, height));
			}
		}

		private void BuildStageMapContent()
		{
			for (var i = _stageMapRoot.childCount - 1; i >= 0; i--)
				Destroy(_stageMapRoot.GetChild(i).gameObject);

			if (_currentQuadrant == null)
				return;

			var summary = CreateRect("Summary", _stageMapRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(760f, 82f));
			var summaryBg = summary.gameObject.AddComponent<Image>();
			summaryBg.color = new Color(0.05f, 0.06f, 0.08f, 0.72f);
			var summaryOutline = summary.gameObject.AddComponent<Outline>();
			summaryOutline.effectColor = GetQuadrantColor(_currentQuadrant.Id);
			summaryOutline.effectDistance = new Vector2(1f, -1f);
			summaryOutline.useGraphicAlpha = true;

			var summaryTitle = CreateText("SummaryTitle", summary, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(700f, 26f), 22f, $"{_currentQuadrant.DisplayName} CAMPAIGN");
			summaryTitle.color = new Color(0.98f, 0.84f, 0.38f, 1f);
			summaryTitle.fontStyle = FontStyles.Bold;
			summaryTitle.alignment = TextAlignmentOptions.Center;

			var summaryMeta = CreateText("SummaryMeta", summary, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(700f, 28f), 13f, $"{_currentQuadrant.ThemeName}  •  8 Encounters  •  {_currentQuadrant.RewardText}");
			summaryMeta.alignment = TextAlignmentOptions.Center;
			summaryMeta.color = new Color(0.9f, 0.9f, 0.9f, 0.94f);

			var track = CreateRect("StageTrack", _stageMapRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -18f), new Vector2(790f, 286f));
			var trackBg = track.gameObject.AddComponent<Image>();
			trackBg.color = new Color(0.03f, 0.03f, 0.04f, 0.18f);

			var nodeGrid = CreateRect("NodeGrid", track, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 34f), new Vector2(748f, 214f));
			var nodeGridImage = nodeGrid.gameObject.AddComponent<Image>();
			nodeGridImage.color = new Color(0.02f, 0.03f, 0.04f, 0.12f);

			var divider = CreateRect("Divider", track, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(0f, 2f));
			var dividerImage = divider.gameObject.AddComponent<Image>();
			dividerImage.color = new Color(0.95f, 0.75f, 0.2f, 0.22f);
			var dividerRect = divider.GetComponent<RectTransform>();
			dividerRect.anchorMin = new Vector2(0.08f, 0.5f);
			dividerRect.anchorMax = new Vector2(0.92f, 0.5f);
			dividerRect.sizeDelta = new Vector2(0f, 2f);

			for (var i = 0; i < _currentQuadrant.Stages.Count; i++)
			{
				var stage = _currentQuadrant.Stages[i];
				var topRow = i < 4;
				var column = topRow ? i : i - 4;
				var x = -282f + column * 188f;
				var y = topRow ? 54f : -54f;
				CreateStageNode(nodeGrid, _currentQuadrant, stage, new Vector2(x, y), GetStageNodeState(i));
			}

			BuildStageDetailPanel(track);
			UpdateStageDetails(_currentQuadrant, _currentQuadrant.Stages[0], GetStageNodeState(0));
		}

		private void CreateQuadrantCard(Transform parent, CampaignQuadrantDefinition quadrant, Vector2 anchoredPosition, Vector2 size)
		{
			var go = new GameObject("QuadrantCard_" + quadrant.Id, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
			go.transform.SetParent(parent, false);

			var rect = go.GetComponent<RectTransform>();
			rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.anchoredPosition = anchoredPosition;
			rect.sizeDelta = size;

			var image = go.GetComponent<Image>();
			image.color = quadrant.IsLocked ? new Color(0.11f, 0.11f, 0.12f, 0.95f) : new Color(0.08f, 0.08f, 0.09f, 0.9f);

			var outerGlow = go.gameObject.AddComponent<Outline>();
			outerGlow.effectColor = quadrant.IsLocked ? new Color(0.44f, 0.44f, 0.46f, 0.72f) : new Color(1f, 0.34f, 0.05f, 0.85f);
			outerGlow.effectDistance = new Vector2(3.5f, -3.5f);
			outerGlow.useGraphicAlpha = true;

			var innerOutline = go.gameObject.AddComponent<Shadow>();
			innerOutline.effectColor = quadrant.IsLocked ? new Color(0f, 0f, 0f, 0.5f) : new Color(1f, 0.76f, 0.18f, 0.72f);
			innerOutline.effectDistance = new Vector2(0f, 0f);
			innerOutline.useGraphicAlpha = true;

			var button = go.GetComponent<Button>();
			button.interactable = !quadrant.IsLocked;
			button.onClick.AddListener(() =>
			{
				if (quadrant.IsLocked)
				{
					UIManager.ShowWarningDialog("Clear all four quadrants to unlock Vulcan City.");
					return;
				}

				ShowStageMap(quadrant);
			});

			var isVulcanCity = quadrant.Id == "vulcan_city";
			var frameSprite = Resources.Load<Sprite>(CardFrameResource);
			var outerHeat = CreateRect("OuterHeat", go.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size.x + 34f, size.y + 34f));
			var outerHeatImage = outerHeat.gameObject.AddComponent<Image>();
			outerHeatImage.color = quadrant.IsLocked ? new Color(0f, 0f, 0f, 0f) : new Color(1f, 0.36f, 0.06f, 0.12f);
			var outerHeatOutline = outerHeat.gameObject.AddComponent<Outline>();
			outerHeatOutline.effectColor = new Color(1f, 0.42f, 0.08f, 0.55f);
			outerHeatOutline.effectDistance = new Vector2(10f, -10f);
			outerHeatOutline.useGraphicAlpha = true;
			outerHeat.transform.SetAsFirstSibling();
			if (frameSprite)
			{
				var fireFrame = CreateRect("FireFrame", go.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size.x + 22f, size.y + 22f));
				var fireFrameImage = fireFrame.gameObject.AddComponent<Image>();
				fireFrameImage.sprite = frameSprite;
				fireFrameImage.type = Image.Type.Sliced;
				fireFrameImage.color = quadrant.IsLocked ? new Color(0f, 0f, 0f, 0f) : new Color(1f, 0.46f, 0.06f, 0.95f);
				var fireOuterGlow = fireFrame.gameObject.AddComponent<Outline>();
				fireOuterGlow.effectColor = new Color(1f, 0.58f, 0.12f, 0.95f);
				fireOuterGlow.effectDistance = new Vector2(6f, -6f);
				fireOuterGlow.useGraphicAlpha = true;
				var fireInnerGlow = fireFrame.gameObject.AddComponent<Shadow>();
				fireInnerGlow.effectColor = new Color(1f, 0.88f, 0.34f, 0.78f);
				fireInnerGlow.effectDistance = new Vector2(0f, 0f);
				fireInnerGlow.useGraphicAlpha = true;

				var frame = CreateRect("Frame", go.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size.x + 10f, size.y + 10f));
				var frameImage = frame.gameObject.AddComponent<Image>();
				frameImage.sprite = frameSprite;
				frameImage.type = Image.Type.Sliced;
				frameImage.color = quadrant.IsLocked ? new Color(0.72f, 0.72f, 0.72f, 0.82f) : new Color(1f, 0.94f, 0.9f, 1f);
				fireFrame.SetAsLastSibling();
				frame.SetSiblingIndex(fireFrame.GetSiblingIndex() - 1);
			}

			var cardInset = CreateRect("CardInset", go.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size.x - 18f, size.y - 18f));
			var cardInsetImage = cardInset.gameObject.AddComponent<Image>();
			cardInsetImage.color = new Color(0.12f, 0.12f, 0.14f, 0.94f);

			AddFireBorder(cardInset, quadrant.IsLocked, isVulcanCity);

			var artHeight = isVulcanCity ? 38f : 72f;
			var artFrame = CreateRect("ArtFrame", cardInset, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(cardInset.sizeDelta.x - 20f, artHeight));
			var artMask = artFrame.gameObject.AddComponent<Image>();
			artMask.color = new Color(0f, 0f, 0f, 0.55f);

			var art = new GameObject("Art", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
			art.transform.SetParent(artFrame, false);
			var artRect = art.GetComponent<RectTransform>();
			artRect.anchorMin = Vector2.zero;
			artRect.anchorMax = Vector2.one;
			artRect.offsetMin = Vector2.zero;
			artRect.offsetMax = Vector2.zero;
			var artImage = art.GetComponent<RawImage>();
			artImage.texture = Resources.Load<Texture2D>(quadrant.PreviewTextureResource);
			artImage.color = quadrant.IsLocked ? new Color(0.45f, 0.45f, 0.45f, 0.85f) : new Color(1f, 1f, 1f, 0.92f);

			var artShade = CreateFillImage("ArtShade", art.transform, new Color(0.02f, 0.02f, 0.04f, 0.2f));
			artShade.rectTransform.offsetMin = Vector2.zero;
			artShade.rectTransform.offsetMax = Vector2.zero;

			var emberGlow = CreateFillImage("EmberGlow", art.transform, quadrant.IsLocked ? new Color(0f, 0f, 0f, 0f) : new Color(1f, 0.42f, 0.08f, 0.22f));
			emberGlow.rectTransform.anchorMin = new Vector2(0f, 0f);
			emberGlow.rectTransform.anchorMax = new Vector2(1f, 0f);
			emberGlow.rectTransform.pivot = new Vector2(0.5f, 0f);
			emberGlow.rectTransform.sizeDelta = new Vector2(0f, 18f);

			var body = CreateRect("Body", cardInset, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 8f), new Vector2(cardInset.sizeDelta.x - 20f, isVulcanCity ? 40f : 70f));
			var bodyImage = body.gameObject.AddComponent<Image>();
			bodyImage.color = new Color(0.07f, 0.07f, 0.08f, quadrant.IsLocked ? 0.9f : 0.96f);
			var bodyWidth = body.sizeDelta.x - 44f;

			var emblem = new GameObject("Emblem", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
			emblem.transform.SetParent(cardInset, false);
			var emblemRect = emblem.GetComponent<RectTransform>();
			emblemRect.anchorMin = emblemRect.anchorMax = new Vector2(0f, 1f);
			emblemRect.pivot = new Vector2(0.5f, 0.5f);
			emblemRect.anchoredPosition = isVulcanCity ? new Vector2(42f, -28f) : new Vector2(36f, -40f);
			emblemRect.sizeDelta = isVulcanCity ? new Vector2(40f, 40f) : new Vector2(46f, 46f);
			var emblemImage = emblem.GetComponent<RawImage>();
			emblemImage.texture = Resources.Load<Texture2D>(quadrant.EmblemTextureResource);
			emblemImage.color = quadrant.IsLocked ? new Color(0.62f, 0.62f, 0.62f, 0.78f) : new Color(1f, 1f, 1f, 0.96f);

			var nameGlow = CreateFillImage("NameGlow", cardInset, quadrant.IsLocked ? new Color(0f, 0f, 0f, 0f) : new Color(1f, 0.52f, 0.14f, 0.12f));
			nameGlow.rectTransform.anchorMin = new Vector2(0f, 0f);
			nameGlow.rectTransform.anchorMax = new Vector2(1f, 0f);
			nameGlow.rectTransform.pivot = new Vector2(0.5f, 0f);
			nameGlow.rectTransform.anchoredPosition = new Vector2(0f, isVulcanCity ? 42f : 66f);
			nameGlow.rectTransform.sizeDelta = new Vector2(0f, 28f);

			var title = CreateText(
				"Title",
				body,
				new Vector2(0f, 1f),
				new Vector2(0f, 1f),
				new Vector2(0f, 1f),
				new Vector2(18f, isVulcanCity ? 0f : -8f),
				new Vector2(bodyWidth, 28f),
				isVulcanCity ? 23f : 22f,
				quadrant.DisplayName);
			title.alignment = TextAlignmentOptions.TopLeft;
			title.fontStyle = FontStyles.Bold;
			title.color = quadrant.IsLocked ? new Color(0.76f, 0.76f, 0.76f, 0.92f) : new Color(0.98f, 0.84f, 0.38f, 1f);
			title.enableAutoSizing = true;
			title.fontSizeMin = 14f;
			title.fontSizeMax = isVulcanCity ? 23f : 22f;
			title.enableWordWrapping = false;
			title.overflowMode = TextOverflowModes.Ellipsis;

			var subtitle = CreateText(
				"Subtitle",
				body,
				new Vector2(0f, 1f),
				new Vector2(0f, 1f),
				new Vector2(0f, 1f),
				new Vector2(18f, isVulcanCity ? -30f : -31f),
				new Vector2(bodyWidth, isVulcanCity ? 16f : 28f),
				isVulcanCity ? 10.5f : 10.5f,
				isVulcanCity ? "Final 3-stage finale" : quadrant.Subtitle);
			subtitle.alignment = TextAlignmentOptions.TopLeft;
			subtitle.color = new Color(0.9f, 0.9f, 0.9f, 0.94f);
			subtitle.enableAutoSizing = false;
			subtitle.enableWordWrapping = true;
			subtitle.overflowMode = TextOverflowModes.Truncate;

			var reward = CreateText(
				"Reward",
				body,
				new Vector2(0f, 0f),
				new Vector2(0f, 0f),
				new Vector2(0f, 0f),
				new Vector2(18f, isVulcanCity ? 8f : 6f),
				new Vector2(bodyWidth, 16f),
				11.5f,
				quadrant.RewardText.ToUpperInvariant());
			reward.alignment = TextAlignmentOptions.BottomLeft;
			reward.color = quadrant.IsLocked ? new Color(0.78f, 0.78f, 0.78f, 0.86f) : new Color(1f, 0.88f, 0.52f, 0.94f);
			reward.fontStyle = FontStyles.Bold;
			reward.enableAutoSizing = true;
			reward.fontSizeMin = 9.5f;
			reward.fontSizeMax = 11.5f;
			reward.enableWordWrapping = false;
			reward.overflowMode = TextOverflowModes.Ellipsis;

			body.SetAsLastSibling();
			title.transform.SetAsLastSibling();
			subtitle.transform.SetAsLastSibling();
			reward.transform.SetAsLastSibling();
		}

		private void CreateStageNode(Transform parent, CampaignQuadrantDefinition quadrant, CampaignStageDefinition stage, Vector2 anchoredPosition, CampaignNodeState state)
		{
			var go = new GameObject("StageNode_" + stage.Index, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
			go.transform.SetParent(parent, false);

			var rect = go.GetComponent<RectTransform>();
			rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.anchoredPosition = anchoredPosition;
			rect.sizeDelta = new Vector2(168f, 96f);

			var image = go.GetComponent<Image>();
			image.color = state switch
			{
				CampaignNodeState.Current => new Color(0.11f, 0.13f, 0.16f, 0.97f),
				CampaignNodeState.Cleared => new Color(0.08f, 0.12f, 0.1f, 0.94f),
				_ => new Color(0.07f, 0.07f, 0.09f, 0.76f)
			};
			var outline = go.gameObject.AddComponent<Outline>();
			outline.effectColor = state == CampaignNodeState.Locked
				? new Color(0.42f, 0.42f, 0.44f, 0.75f)
				: GetStageColor(stage.VisualType);
			outline.effectDistance = new Vector2(1.5f, -1.5f);
			outline.useGraphicAlpha = true;

			var button = go.GetComponent<Button>();
			button.interactable = state != CampaignNodeState.Locked;
			button.onClick.AddListener(() =>
			{
				UpdateStageDetails(quadrant, stage, state);
				if (state == CampaignNodeState.Locked)
				{
					UIManager.ShowWarningDialog("Clear the current encounter to unlock this node.");
					return;
				}

				UIManager.ShowWarningDialog($"{stage.Title} is ready as a campaign map node. Match launch is the next slice.");
			});

			var trigger = go.AddComponent<EventTrigger>();
			AddPointerTrigger(trigger, EventTriggerType.PointerEnter, () => UpdateStageDetails(quadrant, stage, state));

			var art = CreateRect("Art", go.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(152f, 24f));
			var artFrame = art.gameObject.AddComponent<Image>();
			artFrame.color = new Color(0f, 0f, 0f, 0.38f);
			var artImageGo = new GameObject("ArtImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
			artImageGo.transform.SetParent(art, false);
			var artImageRect = artImageGo.GetComponent<RectTransform>();
			artImageRect.anchorMin = Vector2.zero;
			artImageRect.anchorMax = Vector2.one;
			artImageRect.offsetMin = new Vector2(2f, 2f);
			artImageRect.offsetMax = new Vector2(-2f, -2f);
			var artImage = artImageGo.GetComponent<RawImage>();
			artImage.texture = Resources.Load<Texture2D>(quadrant.PreviewTextureResource);
			artImage.color = state == CampaignNodeState.Locked ? new Color(0.45f, 0.45f, 0.45f, 0.78f) : new Color(1f, 1f, 1f, 0.95f);

			var badge = CreateRect("Badge", go.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(8f, -8f), new Vector2(26f, 26f));
			var badgeImage = badge.gameObject.AddComponent<Image>();
			badgeImage.color = GetStageColor(stage.VisualType);
			var badgeLabel = CreateText("BadgeLabel", badge, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(20f, 18f), 13f, stage.Index.ToString());
			badgeLabel.alignment = TextAlignmentOptions.Center;
			badgeLabel.fontStyle = FontStyles.Bold;
			badgeLabel.color = new Color(0.1f, 0.08f, 0.04f, 1f);

			var typeChip = CreateRect("TypeChip", go.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-8f, -8f), new Vector2(52f, 16f));
			var typeChipImage = typeChip.gameObject.AddComponent<Image>();
			typeChipImage.color = new Color(0f, 0f, 0f, 0.34f);
			var typeChipLabel = CreateText("TypeChipLabel", typeChip, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(46f, 14f), 8f, GetStageTypeLabel(stage.VisualType));
			typeChipLabel.alignment = TextAlignmentOptions.Center;
			typeChipLabel.fontStyle = FontStyles.Bold;
			typeChipLabel.color = new Color(0.94f, 0.94f, 0.94f, 0.92f);

			var stateChip = CreateRect("StateChip", go.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 6f), new Vector2(112f, 16f));
			var stateChipImage = stateChip.gameObject.AddComponent<Image>();
			stateChipImage.color = GetNodeStateColor(state);
			var stateChipLabel = CreateText("StateChipLabel", stateChip, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(106f, 14f), 8f, GetNodeStateLabelShort(state));
			stateChipLabel.alignment = TextAlignmentOptions.Center;
			stateChipLabel.fontStyle = FontStyles.Bold;
			stateChipLabel.color = state == CampaignNodeState.Locked ? new Color(0.86f, 0.86f, 0.88f, 0.9f) : new Color(0.15f, 0.08f, 0.02f, 1f);

			var title = CreateText(
				"Title",
				go.transform,
				new Vector2(0f, 1f),
				new Vector2(0f, 1f),
				new Vector2(0f, 1f),
				new Vector2(10f, -34f),
				new Vector2(146f, 24f),
				13f,
				stage.Title);
			title.alignment = TextAlignmentOptions.TopLeft;
			title.fontStyle = FontStyles.Bold;
			title.color = state == CampaignNodeState.Locked ? new Color(0.76f, 0.76f, 0.78f, 0.9f) : new Color(0.98f, 0.84f, 0.38f, 1f);
			title.enableWordWrapping = false;
			title.overflowMode = TextOverflowModes.Ellipsis;
			title.fontSizeMin = 10f;
			title.enableAutoSizing = true;

			var subtitle = CreateText(
				"Subtitle",
				go.transform,
				new Vector2(0f, 0f),
				new Vector2(0f, 0f),
				new Vector2(0f, 0f),
				new Vector2(10f, 24f),
				new Vector2(146f, 14f),
				9f,
				stage.Subtitle);
			subtitle.alignment = TextAlignmentOptions.TopLeft;
			subtitle.color = new Color(0.88f, 0.88f, 0.88f, state == CampaignNodeState.Locked ? 0.7f : 0.92f);
			subtitle.enableWordWrapping = false;
			subtitle.overflowMode = TextOverflowModes.Ellipsis;
		}

		private void CreateBackButton()
		{
			var buttonGo = Instantiate(_generalButtonPrefab, _panelRoot, false);
			buttonGo.name = "BackButton";

			var rect = buttonGo.GetComponent<RectTransform>();
			rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
			rect.pivot = new Vector2(0.5f, 0f);
			rect.anchoredPosition = new Vector2(0f, 14f);
			rect.sizeDelta = new Vector2(300f, 72f);

			var button = buttonGo.GetComponent<Button>();
			button.onClick.AddListener(HandleBackPressed);

			_backLabel = buttonGo.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
			if (_backLabel != null)
				_backLabel.text = "BACK TO SOLO";
		}

		private void HandleBackPressed()
		{
			if (_currentQuadrant != null)
			{
				ShowQuadrantSelection();
				return;
			}

			Hide();
			SoloAdventuresView.Instance?.Show(Owner);
		}

		private RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
		{
			var go = new GameObject(name, typeof(RectTransform));
			go.transform.SetParent(parent, false);
			var rect = go.GetComponent<RectTransform>();
			rect.anchorMin = anchorMin;
			rect.anchorMax = anchorMax;
			rect.pivot = pivot;
			rect.anchoredPosition = anchoredPosition;
			rect.sizeDelta = sizeDelta;
			return rect;
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
			return image;
		}

		private TextMeshProUGUI CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta, float fontSize, string value)
		{
			var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
			go.transform.SetParent(parent, false);
			var rect = go.GetComponent<RectTransform>();
			rect.anchorMin = anchorMin;
			rect.anchorMax = anchorMax;
			rect.pivot = pivot;
			rect.anchoredPosition = anchoredPosition;
			rect.sizeDelta = sizeDelta;

			var label = go.GetComponent<TextMeshProUGUI>();
			label.text = value;
			label.fontSize = fontSize;
			label.enableWordWrapping = true;
			label.raycastTarget = false;
			label.color = Color.white;
			if (_fontAsset)
				label.font = _fontAsset;
			if (_fontMaterial)
				label.fontSharedMaterial = _fontMaterial;
			return label;
		}

		private static Color GetQuadrantColor(string quadrantId)
		{
			return quadrantId switch
			{
				"boreas" => new Color(0.42f, 0.76f, 1f, 0.88f),
				"arcadia" => new Color(0.42f, 0.9f, 0.52f, 0.88f),
				"notus" => new Color(1f, 0.66f, 0.28f, 0.88f),
				"hades" => new Color(0.76f, 0.46f, 1f, 0.88f),
				_ => new Color(0.92f, 0.72f, 0.25f, 0.88f)
			};
		}

		private static Color GetStageColor(CampaignStageVisualType visualType)
		{
			return visualType switch
			{
				CampaignStageVisualType.Elite => new Color(0.97f, 0.54f, 0.12f, 0.94f),
				CampaignStageVisualType.Epic => new Color(0.92f, 0.18f, 0.18f, 0.94f),
				_ => new Color(0.96f, 0.8f, 0.28f, 0.9f)
			};
		}

		private static string GetStageTypeLabel(CampaignStageVisualType visualType)
		{
			return visualType switch
			{
				CampaignStageVisualType.Elite => "ELITE",
				CampaignStageVisualType.Epic => "EPIC",
				_ => "TRIAL"
			};
		}

		private void AddFireBorder(RectTransform parent, bool isLocked, bool isVulcanCity)
		{
			if (isLocked)
				return;

			CreateEdgeGlow("TopGlow", parent, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(8f, -5f), new Vector2(-8f, 12f), new Color(1f, 0.44f, 0.06f, 0.55f));
			CreateEdgeGlow("BottomGlow", parent, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(8f, -4f), new Vector2(-8f, isVulcanCity ? 22f : 18f), new Color(1f, 0.26f, 0.02f, 0.7f));
			CreateEdgeGlow("LeftGlow", parent, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(-4f, 8f), new Vector2(12f, -8f), new Color(1f, 0.4f, 0.06f, 0.42f));
			CreateEdgeGlow("RightGlow", parent, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-12f, 8f), new Vector2(4f, -8f), new Color(1f, 0.4f, 0.06f, 0.42f));

			var bottomHeat = CreateEdgeGlow("BottomHeat", parent, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(18f, 6f), new Vector2(-18f, isVulcanCity ? 28f : 22f), new Color(1f, 0.72f, 0.16f, 0.36f));
			bottomHeat.transform.SetAsLastSibling();

			CreateCornerEmber("TopLeftEmber", parent, new Vector2(0f, 1f), new Vector2(12f, -12f));
			CreateCornerEmber("TopRightEmber", parent, new Vector2(1f, 1f), new Vector2(-12f, -12f));
			CreateCornerEmber("BottomLeftEmber", parent, new Vector2(0f, 0f), new Vector2(14f, 12f));
			CreateCornerEmber("BottomRightEmber", parent, new Vector2(1f, 0f), new Vector2(-14f, 12f));

			var scorch = CreateFillImage("Scorch", parent, new Color(1f, 0.32f, 0.03f, 0.08f));
			scorch.rectTransform.offsetMin = new Vector2(4f, 4f);
			scorch.rectTransform.offsetMax = new Vector2(-4f, -4f);
			scorch.transform.SetAsFirstSibling();
		}

		private RectTransform CreateEdgeGlow(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
		{
			var glow = CreateRect(name, parent, anchorMin, anchorMax, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
			glow.offsetMin = offsetMin;
			glow.offsetMax = offsetMax;
			var glowImage = glow.gameObject.AddComponent<Image>();
			glowImage.color = color;
			glow.SetAsLastSibling();
			return glow;
		}

		private void CreateCornerEmber(string name, RectTransform parent, Vector2 anchor, Vector2 anchoredPosition)
		{
			var ember = CreateRect(name, parent, anchor, anchor, new Vector2(0.5f, 0.5f), anchoredPosition, new Vector2(34f, 34f));
			var emberImage = ember.gameObject.AddComponent<Image>();
			emberImage.color = new Color(1f, 0.46f, 0.04f, 0.78f);
			var emberOutline = ember.gameObject.AddComponent<Outline>();
			emberOutline.effectColor = new Color(1f, 0.92f, 0.46f, 0.68f);
			emberOutline.effectDistance = new Vector2(4f, -4f);
			emberOutline.useGraphicAlpha = true;
			ember.SetAsLastSibling();
		}

		private void BuildStageDetailPanel(Transform parent)
		{
			var panel = CreateRect("StageDetailPanel", parent, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(748f, 122f));
			var panelImage = panel.gameObject.AddComponent<Image>();
			panelImage.color = new Color(0.03f, 0.035f, 0.045f, 0.92f);
			var outline = panel.gameObject.AddComponent<Outline>();
			outline.effectColor = new Color(0.92f, 0.72f, 0.25f, 0.72f);
			outline.effectDistance = new Vector2(1f, -1f);
			outline.useGraphicAlpha = true;

			var innerShade = CreateFillImage("DetailInnerShade", panel, new Color(0f, 0f, 0f, 0.22f));
			innerShade.rectTransform.offsetMin = new Vector2(8f, 8f);
			innerShade.rectTransform.offsetMax = new Vector2(-8f, -8f);
			innerShade.transform.SetAsFirstSibling();

			var artRect = CreateRect("DetailArt", panel, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(14f, 0f), new Vector2(150f, 84f));
			var artFrame = artRect.gameObject.AddComponent<Image>();
			artFrame.color = new Color(0f, 0f, 0f, 0.42f);
			var artImageGo = new GameObject("DetailArtImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
			artImageGo.transform.SetParent(artRect, false);
			var artRectImage = artImageGo.GetComponent<RectTransform>();
			artRectImage.anchorMin = Vector2.zero;
			artRectImage.anchorMax = Vector2.one;
			artRectImage.offsetMin = new Vector2(2f, 2f);
			artRectImage.offsetMax = new Vector2(-2f, -2f);
			_stageDetailArt = artImageGo.GetComponent<RawImage>();

			_stageDetailTitle = CreateText("DetailTitle", panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(178f, -12f), new Vector2(360f, 28f), 22f, "");
			_stageDetailTitle.fontStyle = FontStyles.Bold;
			_stageDetailTitle.color = new Color(0.98f, 0.84f, 0.38f, 1f);
			_stageDetailTitle.enableAutoSizing = false;
			_stageDetailTitle.alignment = TextAlignmentOptions.TopLeft;
			_stageDetailTitle.enableWordWrapping = false;
			_stageDetailTitle.overflowMode = TextOverflowModes.Ellipsis;

			_stageDetailState = CreateText("DetailState", panel, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-16f, -14f), new Vector2(170f, 20f), 12f, "");
			_stageDetailState.alignment = TextAlignmentOptions.TopRight;
			_stageDetailState.fontStyle = FontStyles.Bold;

			_stageDetailBody = CreateText("DetailBody", panel, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(178f, -46f), new Vector2(534f, 54f), 17f, "");
			_stageDetailBody.color = new Color(0.96f, 0.96f, 0.96f, 0.98f);
			_stageDetailBody.alignment = TextAlignmentOptions.TopLeft;
			_stageDetailBody.enableWordWrapping = true;
			_stageDetailBody.overflowMode = TextOverflowModes.Truncate;
			_stageDetailBody.fontStyle = FontStyles.Normal;
			_stageDetailBody.lineSpacing = 0f;
			_stageDetailBody.wordWrappingRatios = 0.35f;
			_stageDetailBody.margin = new Vector4(0f, 0f, 8f, 0f);

			var helper = CreateText("DetailHelper", panel, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-14f, 12f), new Vector2(210f, 14f), 9f, "Hover a node to inspect it");
			helper.alignment = TextAlignmentOptions.BottomRight;
			helper.color = new Color(0.84f, 0.84f, 0.88f, 0.62f);
		}

		private void UpdateStageDetails(CampaignQuadrantDefinition quadrant, CampaignStageDefinition stage, CampaignNodeState state)
		{
			if (_stageDetailTitle == null)
				return;

			_stageDetailTitle.text = stage.Title;
			_stageDetailBody.text = $"{GetStageFlavor(stage)}\n{GetStageInstruction(quadrant, stage)}";
			_stageDetailState.text = GetNodeStateLabel(state);
			_stageDetailState.color = state == CampaignNodeState.Locked
				? new Color(0.78f, 0.78f, 0.8f, 0.94f)
				: new Color(1f, 0.84f, 0.4f, 0.98f);
			if (_stageDetailArt != null)
				_stageDetailArt.texture = Resources.Load<Texture2D>(quadrant.PreviewTextureResource);
		}

		private void AddPointerTrigger(EventTrigger trigger, EventTriggerType eventType, Action action)
		{
			var entry = new EventTrigger.Entry { eventID = eventType };
			entry.callback.AddListener(_ => action());
			trigger.triggers.Add(entry);
		}

		private CampaignNodeState GetStageNodeState(int index)
		{
			if (index == 0)
				return CampaignNodeState.Current;
			if (index < 0)
				return CampaignNodeState.Cleared;
			return CampaignNodeState.Locked;
		}

		private static Color GetNodeStateColor(CampaignNodeState state)
		{
			return state switch
			{
				CampaignNodeState.Current => new Color(1f, 0.78f, 0.2f, 0.92f),
				CampaignNodeState.Cleared => new Color(0.42f, 0.88f, 0.42f, 0.92f),
				_ => new Color(0.28f, 0.28f, 0.32f, 0.88f)
			};
		}

		private static string GetNodeStateLabel(CampaignNodeState state)
		{
			return state switch
			{
				CampaignNodeState.Current => "CURRENT ENCOUNTER",
				CampaignNodeState.Cleared => "CLEARED",
				_ => "LOCKED"
			};
		}

		private static string GetNodeStateLabelShort(CampaignNodeState state)
		{
			return state switch
			{
				CampaignNodeState.Current => "CURRENT",
				CampaignNodeState.Cleared => "CLEARED",
				_ => "LOCKED"
			};
		}

		private static string GetStageFlavor(CampaignStageDefinition stage)
		{
			return stage.VisualType switch
			{
				CampaignStageVisualType.Elite => "Champion encounter with a tougher battlefield rule.",
				CampaignStageVisualType.Epic => "Boss encounter with a signature campaign effect.",
				_ => "Regular encounter that advances your quadrant run."
			};
		}

		private static string GetStageInstruction(CampaignQuadrantDefinition quadrant, CampaignStageDefinition stage)
		{
			return stage.VisualType switch
			{
				CampaignStageVisualType.Epic => $"Win to claim boss stars and push {quadrant.DisplayName} toward completion.",
				CampaignStageVisualType.Elite => $"Win to earn stars and strengthen your {quadrant.DisplayName} campaign run.",
				_ => $"Win to earn stars and keep your {quadrant.DisplayName} run moving."
			};
		}

		private enum CampaignNodeState
		{
			Current,
			Cleared,
			Locked
		}
	}
}
