using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Campaign;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;
using BerserkV3.Common.Utils;
using BerserkV3.Lobby.Deck;
using BerserkV3.Lobby.MatchMaking.Practice;
using BerserkV3.Lobby.Network;
using BerserkV3.Startup.Authorization;
using Cysharp.Threading.Tasks;
using Global;
using RR.UI.FrameSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
	public class CampaignModeView : BaseView
	{
		private const string CardFrameResource = "UI/button-frame";

		private QuadrantView baseQuadrant;
		private QuadrantView vulcanQuadrant;

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
		private bool _isHeroicMode;
		private CampaignProgressModel _campaignProgress;
		private TextMeshProUGUI _stageDetailTitle;
		private TextMeshProUGUI _stageDetailBody;
		private TextMeshProUGUI _stageDetailInstruction;
		private TextMeshProUGUI _stageDetailState;
		private RawImage _stageDetailArt;
		private RectTransform _deckSelectionOverlay;
		private RectTransform _treasureSelectionOverlay;
		private RectTransform _guideOverlay;
		private RectTransform _rewardsOverlay;
		private RectTransform _battleResultsOverlay;
		private RectTransform _claimToastOverlay;
		private RectTransform _treasureToastOverlay;
		private RectTransform _statusToastOverlay;
		private int _claimToastVersion;
		private int _treasureToastVersion;
		private int _statusToastVersion;
		private CampaignRedirectArg? _pendingCampaignResult;

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

			view.baseQuadrant = Resources.Load<QuadrantView>("Quadrants/BaseQuad_Revamp");
			view.vulcanQuadrant = Resources.Load<QuadrantView>("Quadrants/VulcanQuad_Revamp");

			if (view.baseQuadrant == null)
				Debug.LogError("FAILED TO GET BASE QUAD PREFAB");
			if (view.vulcanQuadrant == null)
				Debug.LogError("FAILED TO GET VULCAN QUAD PREFAB");

			return view;
		}

		protected override void OnInit()
		{
			Instance = this;
		}

		protected override void OnShown()
		{
			base.OnShown();

			LoadCampaignProgressAsync().Forget();
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

		public void SetPendingResult(CampaignRedirectArg? campaignResult)
		{
			_pendingCampaignResult = campaignResult;
		}

		public void ShowQuadrantSelection()
		{
			HideDeckSelection();
			HideTreasureSelection();
			HideBattleResultsOverlay();
			_currentQuadrant = null;
			_isHeroicMode = false;
			_titleLabel.text = "THE QUADRANT TRIALS";
			_subtitleLabel.text = "Choose a quadrant to begin your campaign run.";
			_quadrantsRoot.gameObject.SetActive(true);
			_stageMapRoot.gameObject.SetActive(false);
			_backLabel.text = "BACK TO SOLO";
			BuildQuadrantSelection();
		}

		public void ShowQuadrantProgressView(Quadrant quadrant)
		{
			var definition = CampaignModeSettings.Quadrants.FirstOrDefault(x => x.Quadrant == quadrant);
			if (definition == null)
			{
				ShowQuadrantSelection();
				return;
			}

			ShowStageMap(definition);
		}

		private void ShowStageMap(CampaignQuadrantDefinition quadrant)
		{
			if (quadrant == null)
				return;

			HideDeckSelection();
			HideTreasureSelection();
			_currentQuadrant = quadrant;
			_titleLabel.text = quadrant.DisplayName;
			_subtitleLabel.text = $"{quadrant.Subtitle}  Progress updates after every victory.";
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

			_panelRoot = BuildStretchedRect("PanelRoot", transform);
			_panelBackground = _panelRoot.gameObject.AddComponent<Image>();
			var panelBorder = _panelRoot.gameObject.AddComponent<Outline>();
			panelBorder.effectColor = new Color(0.92f, 0.72f, 0.25f, 0.88f);
			panelBorder.effectDistance = new Vector2(1.2f, -1.2f);
			panelBorder.useGraphicAlpha = true;

			var headerBand = CreateRect("HeaderBand", _panelRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -58f), new Vector2(900f, 110f));
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

			_quadrantsRoot = BuildStretchedRect("QuadrantsRoot", _panelRoot);
			BuildQuadrantSelection();

			_stageMapRoot = BuildStretchedRect("StageMapRoot", _panelRoot);
			_stageMapRoot.gameObject.SetActive(false);

			CreateBackButton();
			CreateGuideButton();
			CreateRewardsButton();
		}

		private void BuildQuadrantSelection()
		{
			Debug.Log("GETTING QUAD PREFABS");
			baseQuadrant = baseQuadrant != null ? baseQuadrant : Resources.Load<QuadrantView>("Quadrants/BaseQuad_Revamp");
			vulcanQuadrant = vulcanQuadrant != null ? vulcanQuadrant : Resources.Load<QuadrantView>("Quadrants/VulcanQuad_Revamp");

			if (baseQuadrant == null)
				Debug.LogError("FAILED TO GET BASE QUAD PREFAB");
			if (vulcanQuadrant == null)
				Debug.LogError("FAILED TO GET VULCAN QUAD PREFAB");

			for (var i = _quadrantsRoot.childCount - 1; i >= 0; i--)
				Destroy(_quadrantsRoot.GetChild(i).gameObject);

			var layoutRoot = BuildStretchedRect("QuadrantGrid", _quadrantsRoot);
			layoutRoot.offsetMin = new Vector2(layoutRoot.offsetMin.x, 100f);
			layoutRoot.offsetMax = new Vector2(layoutRoot.offsetMax.x, -100f);

			var backdrop = layoutRoot.gameObject.AddComponent<Image>();
			backdrop.color = new Color(0.03f, 0.04f, 0.05f, 0.18f);
			backdrop.enabled = false;

			// Layout constants
			const float cardWidth = 380f;
			const float cardHeight = 160f;
			const float gap = 0f;
			const float vulcanHeight = 92f;
			const float vulcanWidth = 620f;

			// Total grid height = row0 + gap + row1
			// Center the 2x2 block + vulcan vertically
			// Top of grid row 0 center = +((cardHeight + vulcanHeight) / 2)
			float totalHeight = cardHeight * 2 + gap + vulcanHeight;
			float topRowY = (totalHeight / 2f) - (cardHeight / 2f);         // center of top row
			float bottomRowY = topRowY - cardHeight - gap;                   // center of bottom row
			float vulcanY = bottomRowY - (cardHeight / 2f) - (vulcanHeight / 2f); // flush below bottom row


			for (var i = 0; i < CampaignModeSettings.Quadrants.Count; i++)
			{
				var quadrant = CampaignModeSettings.Quadrants[i];
				var isVulcanCity = quadrant.Id == "vulcan_city";

				float x, y, width, height;

				if (isVulcanCity)
				{
					x = 0f;
					y = vulcanY;
					width = vulcanWidth;
					height = vulcanHeight;
				}
				else
				{
					var row = i / 2;
					var column = i % 2;
					x = column == 0 ? -(cardWidth / 2f) : (cardWidth / 2f);
					y = row == 0 ? topRowY : bottomRowY;
					width = cardWidth;
					height = cardHeight;
				}

				CreateQuadrantCard(layoutRoot, quadrant, new Vector2(x, y), new Vector2(width, height), GetQuadrantProgress(quadrant.Quadrant));
			}
		}

		private RectTransform BuildStretchedRect(string name, Transform parent)
		{
			return CreateRect(name, parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
		}

		private void BuildStageMapContent()
		{
			for (var i = _stageMapRoot.childCount - 1; i >= 0; i--)
				Destroy(_stageMapRoot.GetChild(i).gameObject);

			if (_currentQuadrant == null)
				return;

			var quadrantProgress = GetQuadrantProgress(_currentQuadrant.Quadrant);
			var clearedCount = _isHeroicMode ? quadrantProgress?.CompletedHeroicStages ?? 0 : quadrantProgress?.CompletedStages ?? 0;
			var totalStages = quadrantProgress?.TotalStages ?? _currentQuadrant.Stages.Count;
			var totalStars = GetDisplayedStageProgresses(quadrantProgress)?.Sum(x => x.Stars) ?? 0;

			var summary = CreateRect("Summary", _stageMapRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(760f, 82f));
			var summaryBg = summary.gameObject.AddComponent<Image>();
			summaryBg.color = new Color(0.05f, 0.06f, 0.08f, 0.72f);
			var summaryOutline = summary.gameObject.AddComponent<Outline>();
			summaryOutline.effectColor = GetQuadrantColor(_currentQuadrant.Id);
			summaryOutline.effectDistance = new Vector2(1f, -1f);
			summaryOutline.useGraphicAlpha = true;

			var summaryTitle = CreateText("SummaryTitle", summary, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(700f, 28f), 24f, $"{_currentQuadrant.DisplayName} CAMPAIGN");
			summaryTitle.color = new Color(0.98f, 0.84f, 0.38f, 1f);
			summaryTitle.fontStyle = FontStyles.Bold;
			summaryTitle.alignment = TextAlignmentOptions.Center;

			var activeTreasures = _campaignProgress?.ActiveTreasures?.Count ?? 0;
			var summaryMeta = CreateText("SummaryMeta", summary, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(724f, 30f), 13.5f, $"{_currentQuadrant.ThemeName}  •  {clearedCount}/{totalStages} cleared  •  {totalStars} stars earned  •  {activeTreasures} treasures");
			summaryMeta.alignment = TextAlignmentOptions.Center;
			summaryMeta.color = new Color(0.9f, 0.9f, 0.9f, 0.94f);

			if (!_currentQuadrant.IsLocked && _currentQuadrant.Quadrant != Quadrant.Vulcan_City)
			{
				var heroicButton = CreateRect("HeroicToggle", summary, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-16f, 0f), new Vector2(148f, 34f));
				var heroicButtonImage = heroicButton.gameObject.AddComponent<Image>();
				var heroicButtonState = quadrantProgress?.IsHeroicUnlocked == true;
				heroicButtonImage.color = _isHeroicMode
					? new Color(0.94f, 0.3f, 0.12f, 0.96f)
					: heroicButtonState
						? new Color(0.22f, 0.12f, 0.08f, 0.94f)
						: new Color(0.18f, 0.18f, 0.2f, 0.82f);
				var heroicButtonOutline = heroicButton.gameObject.AddComponent<Outline>();
				heroicButtonOutline.effectColor = _isHeroicMode
					? new Color(1f, 0.78f, 0.32f, 0.72f)
					: new Color(0.4f, 0.4f, 0.45f, 0.72f);
				heroicButtonOutline.effectDistance = new Vector2(1f, -1f);
				heroicButtonOutline.useGraphicAlpha = true;
				var heroicToggle = heroicButton.gameObject.AddComponent<Button>();
				heroicToggle.interactable = heroicButtonState;
				heroicToggle.targetGraphic = heroicButtonImage;
				heroicToggle.onClick.AddListener(() =>
				{
					_isHeroicMode = !_isHeroicMode;
					BuildStageMapContent();
				});
				var heroicLabel = CreateText("HeroicToggleLabel", heroicButton, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(132f, 20f), 13f, heroicButtonState ? (_isHeroicMode ? "HEROIC" : "NORMAL") : "HEROIC LOCKED");
				heroicLabel.alignment = TextAlignmentOptions.Center;
				heroicLabel.fontStyle = FontStyles.Bold;
				heroicLabel.color = heroicButtonState
					? (_isHeroicMode ? new Color(0.17f, 0.06f, 0.01f, 1f) : new Color(1f, 0.9f, 0.68f, 0.98f))
					: new Color(0.75f, 0.75f, 0.78f, 0.94f);
			}

			var track = CreateRect("StageTrack", _stageMapRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(790f, 286f));
			var trackBg = track.gameObject.AddComponent<Image>();
			trackBg.color = new Color(0.03f, 0.03f, 0.04f, 0.18f);

			var nodeGrid = CreateRect("NodeGrid", track, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 34f), new Vector2(748f, 214f));
			var nodeGridImage = nodeGrid.gameObject.AddComponent<Image>();
			nodeGridImage.color = new Color(0.02f, 0.03f, 0.04f, 0.12f);

			var divider = CreateRect("Divider", track, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 33f), new Vector2(0f, 2f));
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
				var y = topRow ? 54 : -54;
				CreateStageNode(nodeGrid, _currentQuadrant, stage, new Vector2(x, y), GetStageNodeState(stage), GetStageProgress(_currentQuadrant, stage.Index));
			}

			BuildStageDetailPanel(track);
			var initialStage = GetInitialStageDefinition(_currentQuadrant) ?? _currentQuadrant.Stages[0];
			UpdateStageDetails(_currentQuadrant, initialStage, GetStageNodeState(initialStage));
		}

		private void CreateQuadrantCard(Transform parent, CampaignQuadrantDefinition quadrant, Vector2 anchoredPosition, Vector2 size, CampaignQuadrantProgressModel progress)
		{
			var isVulcanCity = quadrant.Id == "vulcan_city";

			var quadView = Instantiate(isVulcanCity ? vulcanQuadrant : baseQuadrant, parent);
			quadView.transform.SetParent(parent, false);

			var rect = quadView.GetComponent<RectTransform>();
			rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.anchoredPosition = anchoredPosition;
			rect.sizeDelta = size;

			var isLocked = progress != null ? !progress.IsUnlocked : quadrant.IsLocked;
			Color col = isLocked ? new Color(0.11f, 0.11f, 0.12f, 0.95f) : new Color(0.6037736f, 0.6037736f, 0.6037736f, 1f);
			quadView.SetBackgroundCol(col);

			quadView.SetButtonAcion(() => //TODO: Check what the void do
			{
				if (isLocked)
				{
					ShowCampaignStatus("VULCAN CITY LOCKED", "Clear all four quadrants in Normal to unlock Vulcan City.");
					return;
				}

				ShowStageMap(quadrant);
			},
			!isLocked);

			quadView.SetBaseQuadrant(quadrant);
			quadView.SetSubtitle(isVulcanCity ? "Final 3-stage finale" : (progress?.Description ?? quadrant.Subtitle));
			var rewardText = isVulcanCity
				? quadrant.RewardText
				: progress != null
					? $"{progress.CompletedStages}/{progress.TotalStages} cleared  •  {progress.Stages.Sum(x => x.Stars)} stars"
					: quadrant.RewardText;
			quadView.SetReward(rewardText);
		}

		private void CreateStageNode(Transform parent, CampaignQuadrantDefinition quadrant, CampaignStageDefinition stage, Vector2 anchoredPosition, CampaignNodeState state, CampaignStageProgressModel stageProgress)
		{
			var go = new GameObject("StageNode_" + stage.Index, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
			go.transform.SetParent(parent, false);

			var rect = go.GetComponent<RectTransform>();
			rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.anchoredPosition = anchoredPosition;
			rect.sizeDelta = new Vector2(176f, 104f);

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
			button.onClick.AddListener(() => HandleStageSelectedAsync(quadrant, stage, state).Forget());

			var trigger = go.AddComponent<EventTrigger>();
			AddPointerTrigger(trigger, EventTriggerType.PointerEnter, () => UpdateStageDetails(quadrant, stage, state));

			var art = CreateRect("Art", go.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(160f, 26f));
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

			var badge = CreateRect("Badge", go.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(8f, -8f), new Vector2(28f, 28f));
			var badgeImage = badge.gameObject.AddComponent<Image>();
			badgeImage.color = GetStageColor(stage.VisualType);
			var badgeLabel = CreateText("BadgeLabel", badge, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(22f, 18f), 13.5f, stage.Index.ToString());
			badgeLabel.alignment = TextAlignmentOptions.Center;
			badgeLabel.fontStyle = FontStyles.Bold;
			badgeLabel.color = new Color(0.1f, 0.08f, 0.04f, 1f);

			var typeChip = CreateRect("TypeChip", go.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-8f, -8f), new Vector2(56f, 17f));
			var typeChipImage = typeChip.gameObject.AddComponent<Image>();
			typeChipImage.color = new Color(0f, 0f, 0f, 0.34f);
			var typeChipLabel = CreateText("TypeChipLabel", typeChip, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(50f, 14f), 8.5f, GetStageTypeLabel(stage.VisualType));
			typeChipLabel.alignment = TextAlignmentOptions.Center;
			typeChipLabel.fontStyle = FontStyles.Bold;
			typeChipLabel.color = new Color(0.94f, 0.94f, 0.94f, 0.92f);

			var stateChip = CreateRect("StateChip", go.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 8f), new Vector2(118f, 18f));
			var stateChipImage = stateChip.gameObject.AddComponent<Image>();
			stateChipImage.color = GetNodeStateColor(state);
			var stateChipLabel = CreateText("StateChipLabel", stateChip, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(110f, 14f), 8.5f, GetNodeStateLabelShort(state));
			stateChipLabel.alignment = TextAlignmentOptions.Center;
			stateChipLabel.fontStyle = FontStyles.Bold;
			stateChipLabel.color = state == CampaignNodeState.Locked ? new Color(0.86f, 0.86f, 0.88f, 0.9f) : new Color(0.15f, 0.08f, 0.02f, 1f);

			var title = CreateText(
				"Title",
				go.transform,
				new Vector2(0f, 1f),
				new Vector2(0f, 1f),
				new Vector2(0f, 1f),
				new Vector2(12f, -39f),
				new Vector2(152f, 26f),
				14f,
				stage.Title);
			title.alignment = TextAlignmentOptions.TopLeft;
			title.fontStyle = FontStyles.Bold;
			title.color = state == CampaignNodeState.Locked ? new Color(0.76f, 0.76f, 0.78f, 0.9f) : new Color(0.98f, 0.84f, 0.38f, 1f);
			title.enableWordWrapping = false;
			title.overflowMode = TextOverflowModes.Ellipsis;
			title.fontSizeMin = 11f;
			title.enableAutoSizing = true;

			if (stageProgress?.Stars > 0)
				CreateStageStars(go.transform, stageProgress.Stars);

			var subtitle = CreateText(
				"Subtitle",
				go.transform,
				new Vector2(0f, 0f),
				new Vector2(0f, 0f),
				new Vector2(0f, 0f),
				new Vector2(12f, 30f),
				new Vector2(152f, 16f),
				9.5f,
				stage.Subtitle);
			subtitle.alignment = TextAlignmentOptions.TopLeft;
			subtitle.color = new Color(0.88f, 0.88f, 0.88f, state == CampaignNodeState.Locked ? 0.7f : 0.92f);
			subtitle.enableWordWrapping = false;
			subtitle.overflowMode = TextOverflowModes.Ellipsis;
		}

		private void CreateStageStars(Transform parent, int stars)
		{
			var clampedStars = Mathf.Clamp(stars, 0, 3);
			if (clampedStars <= 0)
				return;

			var row = CreateRect("Stars", parent, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(12f, 32f), new Vector2(54f, 10f));
			for (var i = 0; i < 3; i++)
			{
				var pip = CreateRect($"Star_{i}", row, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(i * 18f, 0f), new Vector2(12f, 12f));
				var pipImage = pip.gameObject.AddComponent<Image>();
				pipImage.color = i < clampedStars
					? new Color(1f, 0.82f, 0.24f, 0.98f)
					: new Color(0.34f, 0.34f, 0.38f, 0.75f);
				var pipOutline = pip.gameObject.AddComponent<Outline>();
				pipOutline.effectColor = i < clampedStars
					? new Color(1f, 0.95f, 0.62f, 0.72f)
					: new Color(0f, 0f, 0f, 0.3f);
				pipOutline.effectDistance = new Vector2(1f, -1f);
				pipOutline.useGraphicAlpha = true;
			}
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
			buttonGo.transform.SetAsLastSibling();
		}

		private void HandleBackPressed()
		{
			if (_rewardsOverlay)
			{
				HideRewardsOverlay();
				return;
			}

			if (_guideOverlay)
			{
				HideGuideOverlay();
				return;
			}

			if (_battleResultsOverlay)
			{
				HideBattleResultsOverlay();
				if (_campaignProgress?.PendingTreasureChoices?.Count > 0)
					ShowTreasureSelection();
				return;
			}

			if (_treasureSelectionOverlay)
			{
				ShowCampaignStatus("TREASURE REQUIRED", "Choose a treasure reward before leaving this campaign step.");
				return;
			}

			if (_deckSelectionOverlay)
			{
				HideDeckSelection();
				return;
			}

			if (_currentQuadrant != null)
			{
				ShowQuadrantSelection();
				return;
			}

			Hide();
		}

		private void CreateGuideButton()
		{
			var buttonGo = new GameObject("GuideButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
			buttonGo.transform.SetParent(_panelRoot, false);

			var rect = buttonGo.GetComponent<RectTransform>();
			rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
			rect.pivot = new Vector2(1f, 1f);
			rect.anchoredPosition = new Vector2(-10f, -10f);
			rect.sizeDelta = new Vector2(160f, 42f);

			var image = buttonGo.GetComponent<Image>();
			image.color = new Color(0.12f, 0.12f, 0.14f, 0.9f);
			var outline = buttonGo.gameObject.AddComponent<Outline>();
			outline.effectColor = new Color(0.95f, 0.75f, 0.2f, 0.78f);
			outline.effectDistance = new Vector2(1f, -1f);
			outline.useGraphicAlpha = true;

			var button = buttonGo.GetComponent<Button>();
			button.targetGraphic = image;
			button.onClick.AddListener(ShowGuideOverlay);

			var label = CreateText("GuideButtonLabel", buttonGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(164f, 22f), 11f, "HOW CAMPAIGN WORKS");
			label.fontStyle = FontStyles.Bold;
			label.alignment = TextAlignmentOptions.Center;
			label.color = new Color(1f, 0.9f, 0.66f, 0.98f);
		}

		private void CreateRewardsButton()
		{
			var buttonGo = new GameObject("RewardsButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
			buttonGo.transform.SetParent(_panelRoot, false);

			var rect = buttonGo.GetComponent<RectTransform>();
			rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
			rect.pivot = new Vector2(0f, 1f);
			rect.anchoredPosition = new Vector2(10f, -10f);
			rect.sizeDelta = new Vector2(160f, 42f);

			var image = buttonGo.GetComponent<Image>();
			image.color = new Color(0.12f, 0.12f, 0.14f, 0.9f);
			var outline = buttonGo.gameObject.AddComponent<Outline>();
			outline.effectColor = new Color(0.95f, 0.75f, 0.2f, 0.78f);
			outline.effectDistance = new Vector2(1f, -1f);
			outline.useGraphicAlpha = true;

			var button = buttonGo.GetComponent<Button>();
			button.targetGraphic = image;
			button.onClick.AddListener(ShowRewardsOverlay);

			var label = CreateText("RewardsButtonLabel", buttonGo.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(176f, 22f), 11f, "REWARDS & PROGRESS");
			label.fontStyle = FontStyles.Bold;
			label.alignment = TextAlignmentOptions.Center;
			label.color = new Color(1f, 0.9f, 0.66f, 0.98f);
		}

		private void ShowGuideOverlay()
		{
			if (_guideOverlay)
			{
				_guideOverlay.SetAsLastSibling();
				return;
			}

			_guideOverlay = CreateRect("CampaignGuideOverlay", _panelRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(860f, 556f));
			var overlayImage = _guideOverlay.gameObject.AddComponent<Image>();
			overlayImage.color = new Color(0.02f, 0.025f, 0.035f, 0.975f);
			var overlayOutline = _guideOverlay.gameObject.AddComponent<Outline>();
			overlayOutline.effectColor = new Color(0.95f, 0.75f, 0.2f, 0.88f);
			overlayOutline.effectDistance = new Vector2(2f, -2f);
			overlayOutline.useGraphicAlpha = true;

			var title = CreateText("GuideTitle", _guideOverlay, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(720f, 30f), 28f, "HOW CAMPAIGN WORKS");
			title.alignment = TextAlignmentOptions.Center;
			title.fontStyle = FontStyles.Bold;
			title.color = new Color(0.98f, 0.84f, 0.38f, 1f);

			var subtitle = CreateText("GuideSubtitle", _guideOverlay, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -58f), new Vector2(740f, 24f), 15f, "Fight through four quadrants, earn stars and treasures, unlock Heroic, then open Vulcan City.");
			subtitle.alignment = TextAlignmentOptions.Center;
			subtitle.color = new Color(0.92f, 0.92f, 0.94f, 0.94f);

			var grid = CreateRect("GuideGrid", _guideOverlay, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 16f), new Vector2(776f, 370f));
			CreateGuideSection(grid, "GuideQuadrants", new Vector2(-196f, 108f), "THE MAP", "Each core quadrant has 8 encounters: 5 trials, 2 elites, and 1 epic boss. Win the current node to unlock the next one.");
			CreateGuideSection(grid, "GuideEncounterTypes", new Vector2(196f, 108f), "ENCOUNTER TYPES", "Trials are standard fights. Elites are harder checkpoints. Epic bosses are the big finish. Special and Puzzle nodes have custom opening rules.");
			CreateGuideSection(grid, "GuideStars", new Vector2(-196f, -6f), "STARS", "Every cleared stage can earn 1 to 3 stars. Winning matters first. Cleaner, stronger finishes earn more.");
			CreateGuideSection(grid, "GuideTreasures", new Vector2(196f, -6f), "TREASURES", "Some victories let you choose 1 treasure. Treasure bonuses stay active for the run and show during Campaign battles.");
			CreateGuideSection(grid, "GuideHeroic", new Vector2(-196f, -120f), "HEROIC", "Clear an entire quadrant in Normal to unlock Heroic for that quadrant. Heroic enemies start tougher and use harsher setup rules.");
			CreateGuideSection(grid, "GuideFinale", new Vector2(196f, -120f), "VULCAN CITY", "Clear Boreas, Arcadia, Notus, and Hades to unlock the three-stage finale: Forge Master, The Champion, and Vulcan.");

			var footer = CreateText("GuideFooter", _guideOverlay, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 58f), new Vector2(740f, 34f), 13f, "Hover a node to inspect it, pick a deck before battle, claim milestones in Rewards & Progress, and keep pushing toward Vulcan City.");
			footer.alignment = TextAlignmentOptions.Center;
			footer.color = new Color(0.9f, 0.9f, 0.92f, 0.86f);

			var closeButton = CreateRect("GuideCloseButton", _guideOverlay, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(220f, 46f));
			var closeImage = closeButton.gameObject.AddComponent<Image>();
			closeImage.color = new Color(1f, 0.78f, 0.2f, 0.96f);
			var close = closeButton.gameObject.AddComponent<Button>();
			close.targetGraphic = closeImage;
			close.onClick.AddListener(HideGuideOverlay);
			var closeLabel = CreateText("GuideCloseLabel", closeButton, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(160f, 22f), 15f, "START CAMPAIGN");
			closeLabel.alignment = TextAlignmentOptions.Center;
			closeLabel.fontStyle = FontStyles.Bold;
			closeLabel.color = new Color(0.18f, 0.1f, 0.02f, 1f);

			_guideOverlay.SetAsLastSibling();
		}

		private void HideGuideOverlay()
		{
			if (!_guideOverlay)
				return;

			Destroy(_guideOverlay.gameObject);
			_guideOverlay = null;
		}

		private void ShowRewardsOverlay()
		{
			if (_rewardsOverlay)
			{
				_rewardsOverlay.SetAsLastSibling();
				return;
			}

			if (_campaignProgress == null)
			{
				ShowCampaignStatus("CAMPAIGN STILL LOADING", "Campaign progress is still loading. Try again in a moment.");
				return;
			}

			_rewardsOverlay = CreateRect("CampaignRewardsOverlay", _panelRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(880f, 560f));
			var overlayImage = _rewardsOverlay.gameObject.AddComponent<Image>();
			overlayImage.color = new Color(0.02f, 0.025f, 0.035f, 0.98f);
			var overlayOutline = _rewardsOverlay.gameObject.AddComponent<Outline>();
			overlayOutline.effectColor = new Color(0.95f, 0.75f, 0.2f, 0.88f);
			overlayOutline.effectDistance = new Vector2(2f, -2f);
			overlayOutline.useGraphicAlpha = true;

			var title = CreateText("RewardsTitle", _rewardsOverlay, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(720f, 30f), 28f, "CAMPAIGN REWARDS & PROGRESS");
			title.alignment = TextAlignmentOptions.Center;
			title.fontStyle = FontStyles.Bold;
			title.color = new Color(0.98f, 0.84f, 0.38f, 1f);

			var subtitle = CreateText("RewardsSubtitle", _rewardsOverlay, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -58f), new Vector2(760f, 22f), 14f, "Track milestone rewards, active treasures, and what each quadrant is building toward.");
			subtitle.alignment = TextAlignmentOptions.Center;
			subtitle.color = new Color(0.92f, 0.92f, 0.94f, 0.94f);

			var summary = CreateRect("RewardsSummary", _rewardsOverlay, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -112f), new Vector2(780f, 58f));
			var summaryImage = summary.gameObject.AddComponent<Image>();
			summaryImage.color = new Color(0.08f, 0.09f, 0.12f, 0.92f);
			var summaryOutline = summary.gameObject.AddComponent<Outline>();
			summaryOutline.effectColor = new Color(0.95f, 0.75f, 0.2f, 0.38f);
			summaryOutline.effectDistance = new Vector2(1f, -1f);
			summaryOutline.useGraphicAlpha = true;

			var summaryText = CreateText("RewardsSummaryText", summary, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(736f, 26f), 16f,
				$"{_campaignProgress.CompletedCoreQuadrants}/4 core quadrants cleared  •  {_campaignProgress.TotalStars} stars  •  {_campaignProgress.ActiveTreasures.Count} active treasures  •  {_campaignProgress.ClaimedMilestoneExperience} BXP reserved  •  {_campaignProgress.ClaimedMilestoneCardCount} card rewards reserved");
			summaryText.alignment = TextAlignmentOptions.Center;
			summaryText.fontStyle = FontStyles.Bold;
			summaryText.color = new Color(0.98f, 0.9f, 0.72f, 0.98f);

			var milestonesPanel = CreateRect("MilestonesPanel", _rewardsOverlay, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(28f, -18f), new Vector2(454f, 346f));
			var milestonesImage = milestonesPanel.gameObject.AddComponent<Image>();
			milestonesImage.color = new Color(0.08f, 0.09f, 0.11f, 0.92f);
			var milestonesLabel = CreateText("MilestonesLabel", milestonesPanel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(360f, 24f), 20f, "MILESTONE TRACK");
			milestonesLabel.alignment = TextAlignmentOptions.Center;
			milestonesLabel.fontStyle = FontStyles.Bold;
			milestonesLabel.color = new Color(0.98f, 0.84f, 0.38f, 1f);

			for (var i = 0; i < _campaignProgress.Milestones.Count; i++)
			{
				CreateMilestoneRow(milestonesPanel, _campaignProgress.Milestones[i], i);
			}

			var sidePanel = CreateRect("RewardsSidePanel", _rewardsOverlay, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-28f, -18f), new Vector2(340f, 346f));
			var sideImage = sidePanel.gameObject.AddComponent<Image>();
			sideImage.color = new Color(0.08f, 0.09f, 0.11f, 0.92f);

			var treasuresTitle = CreateText("TreasuresTitle", sidePanel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(260f, 24f), 18f, "ACTIVE TREASURES");
			treasuresTitle.alignment = TextAlignmentOptions.Center;
			treasuresTitle.fontStyle = FontStyles.Bold;
			treasuresTitle.color = new Color(0.98f, 0.84f, 0.38f, 1f);

			if (_campaignProgress.ActiveTreasures.Count == 0)
			{
				var emptyTreasures = CreateText("NoTreasures", sidePanel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -52f), new Vector2(270f, 36f), 14f, "No active treasures yet. Treasures appear after key campaign wins.");
				emptyTreasures.alignment = TextAlignmentOptions.Center;
				emptyTreasures.color = new Color(0.9f, 0.9f, 0.92f, 0.86f);
			}
			else
			{
				for (var i = 0; i < _campaignProgress.ActiveTreasures.Count; i++)
				{
					CreateTreasureSummaryRow(sidePanel, _campaignProgress.ActiveTreasures[i], i);
				}
			}

			var quadrantTitle = CreateText("QuadrantRewardsTitle", sidePanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(260f, 24f), 18f, "QUADRANT PAYOFFS");
			quadrantTitle.alignment = TextAlignmentOptions.Center;
			quadrantTitle.fontStyle = FontStyles.Bold;
			quadrantTitle.color = new Color(0.98f, 0.84f, 0.38f, 1f);

			for (var i = 0; i < _campaignProgress.Quadrants.Count; i++)
			{
				CreateQuadrantSummaryRow(sidePanel, _campaignProgress.Quadrants[i], i);
			}

			var unlocksTitle = CreateText("RewardsUnlocksTitle", sidePanel, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 92f), new Vector2(260f, 22f), 16f, "CLAIMED PLACEHOLDERS");
			unlocksTitle.alignment = TextAlignmentOptions.Center;
			unlocksTitle.fontStyle = FontStyles.Bold;
			unlocksTitle.color = new Color(0.98f, 0.84f, 0.38f, 1f);

			var unlocksBody = CreateText("RewardsUnlocksBody", sidePanel, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 48f), new Vector2(284f, 70f), 12f,
				BuildUnlocksSummaryText());
			unlocksBody.alignment = TextAlignmentOptions.Center;
			unlocksBody.color = new Color(0.92f, 0.92f, 0.95f, 0.92f);
			unlocksBody.enableWordWrapping = true;
			unlocksBody.overflowMode = TextOverflowModes.Ellipsis;

			var closeButton = CreateRect("RewardsCloseButton", _rewardsOverlay, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(220f, 46f));
			var closeImage = closeButton.gameObject.AddComponent<Image>();
			closeImage.color = new Color(1f, 0.78f, 0.2f, 0.96f);
			var close = closeButton.gameObject.AddComponent<Button>();
			close.targetGraphic = closeImage;
			close.onClick.AddListener(HideRewardsOverlay);
			var closeLabel = CreateText("RewardsCloseLabel", closeButton, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(160f, 22f), 15f, "CLOSE");
			closeLabel.alignment = TextAlignmentOptions.Center;
			closeLabel.fontStyle = FontStyles.Bold;
			closeLabel.color = new Color(0.18f, 0.1f, 0.02f, 1f);

			_rewardsOverlay.SetAsLastSibling();
		}

		private void HideRewardsOverlay()
		{
			if (!_rewardsOverlay)
				return;

			Destroy(_rewardsOverlay.gameObject);
			_rewardsOverlay = null;
		}

		private void ShowMilestoneClaimToast(CampaignMilestoneRewardModel milestone)
		{
			HideClaimToast();

			_claimToastVersion++;
			var currentVersion = _claimToastVersion;

			_claimToastOverlay = CreateRect("CampaignClaimToast", _panelRoot, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-26f, -26f), new Vector2(332f, 154f));
			var toastImage = _claimToastOverlay.gameObject.AddComponent<Image>();
			toastImage.color = new Color(0.05f, 0.08f, 0.06f, 0.975f);
			var toastOutline = _claimToastOverlay.gameObject.AddComponent<Outline>();
			toastOutline.effectColor = new Color(0.98f, 0.84f, 0.38f, 0.9f);
			toastOutline.effectDistance = new Vector2(2f, -2f);
			toastOutline.useGraphicAlpha = true;

			var title = CreateText("ClaimToastTitle", _claimToastOverlay, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(260f, 24f), 18f, "MILESTONE CLAIMED");
			title.alignment = TextAlignmentOptions.Center;
			title.fontStyle = FontStyles.Bold;
			title.color = new Color(0.6f, 1f, 0.56f, 0.98f);

			var milestoneTitle = CreateText("ClaimToastMilestone", _claimToastOverlay, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -46f), new Vector2(284f, 22f), 16f, milestone?.Title?.ToUpperInvariant() ?? "CAMPAIGN REWARD");
			milestoneTitle.alignment = TextAlignmentOptions.Center;
			milestoneTitle.fontStyle = FontStyles.Bold;
			milestoneTitle.color = new Color(0.98f, 0.9f, 0.72f, 0.98f);
			milestoneTitle.enableWordWrapping = false;
			milestoneTitle.overflowMode = TextOverflowModes.Ellipsis;

			var body = CreateText("ClaimToastBody", _claimToastOverlay, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -4f), new Vector2(286f, 52f), 14f, BuildMilestoneClaimRewardText(milestone));
			body.alignment = TextAlignmentOptions.Center;
			body.color = new Color(0.96f, 0.96f, 0.98f, 0.98f);
			body.enableWordWrapping = true;
			body.overflowMode = TextOverflowModes.Ellipsis;

			var footer = CreateText("ClaimToastFooter", _claimToastOverlay, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(288f, 18f), 10.5f, "Placeholder reward recorded only. Live grants come later.");
			footer.alignment = TextAlignmentOptions.Center;
			footer.color = new Color(0.9f, 0.9f, 0.92f, 0.82f);

			_claimToastOverlay.SetAsLastSibling();
			AutoHideClaimToast(currentVersion).Forget();
		}

		private void ShowTreasureClaimToast(CampaignTreasureChoiceModel treasure)
		{
			HideTreasureToast();

			_treasureToastVersion++;
			var currentVersion = _treasureToastVersion;

			_treasureToastOverlay = CreateRect("CampaignTreasureToast", _panelRoot, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-26f, -194f), new Vector2(332f, 146f));
			var toastImage = _treasureToastOverlay.gameObject.AddComponent<Image>();
			toastImage.color = new Color(0.08f, 0.06f, 0.03f, 0.975f);
			var toastOutline = _treasureToastOverlay.gameObject.AddComponent<Outline>();
			toastOutline.effectColor = new Color(1f, 0.7f, 0.2f, 0.92f);
			toastOutline.effectDistance = new Vector2(2f, -2f);
			toastOutline.useGraphicAlpha = true;

			var title = CreateText("TreasureToastTitle", _treasureToastOverlay, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(260f, 24f), 18f, "TREASURE SECURED");
			title.alignment = TextAlignmentOptions.Center;
			title.fontStyle = FontStyles.Bold;
			title.color = new Color(1f, 0.9f, 0.56f, 0.98f);

			var treasureTitle = CreateText("TreasureToastTreasure", _treasureToastOverlay, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -46f), new Vector2(284f, 22f), 16f, treasure?.Title?.ToUpperInvariant() ?? "CAMPAIGN TREASURE");
			treasureTitle.alignment = TextAlignmentOptions.Center;
			treasureTitle.fontStyle = FontStyles.Bold;
			treasureTitle.color = new Color(0.98f, 0.9f, 0.72f, 0.98f);
			treasureTitle.enableWordWrapping = false;
			treasureTitle.overflowMode = TextOverflowModes.Ellipsis;

			var body = CreateText("TreasureToastBody", _treasureToastOverlay, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -2f), new Vector2(286f, 40f), 14f, BuildTreasureBonusText(treasure));
			body.alignment = TextAlignmentOptions.Center;
			body.color = new Color(0.96f, 0.96f, 0.98f, 0.98f);
			body.fontStyle = FontStyles.Bold;

			var footer = CreateText("TreasureToastFooter", _treasureToastOverlay, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(288f, 18f), 10.5f, "This bonus now applies to the rest of the current campaign run.");
			footer.alignment = TextAlignmentOptions.Center;
			footer.color = new Color(0.9f, 0.9f, 0.92f, 0.82f);

			_treasureToastOverlay.SetAsLastSibling();
			AutoHideTreasureToast(currentVersion).Forget();
		}

		private void ShowCampaignStatus(string title, string body, bool isError = false)
		{
			HideStatusToast();

			_statusToastVersion++;
			var currentVersion = _statusToastVersion;

			_statusToastOverlay = CreateRect("CampaignStatusToast", _panelRoot, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-26f, -362f), new Vector2(348f, 138f));
			var toastImage = _statusToastOverlay.gameObject.AddComponent<Image>();
			toastImage.color = isError ? new Color(0.18f, 0.04f, 0.04f, 0.97f) : new Color(0.06f, 0.08f, 0.1f, 0.97f);
			var toastOutline = _statusToastOverlay.gameObject.AddComponent<Outline>();
			toastOutline.effectColor = isError ? new Color(1f, 0.36f, 0.28f, 0.86f) : new Color(0.98f, 0.76f, 0.2f, 0.84f);
			toastOutline.effectDistance = new Vector2(2f, -2f);
			toastOutline.useGraphicAlpha = true;

			var titleLabel = CreateText("CampaignStatusTitle", _statusToastOverlay, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(296f, 24f), 17f, title);
			titleLabel.alignment = TextAlignmentOptions.Center;
			titleLabel.fontStyle = FontStyles.Bold;
			titleLabel.color = isError ? new Color(1f, 0.78f, 0.72f, 1f) : new Color(0.98f, 0.84f, 0.38f, 1f);

			var bodyLabel = CreateText("CampaignStatusBody", _statusToastOverlay, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -2f), new Vector2(302f, 54f), 14.5f, body);
			bodyLabel.alignment = TextAlignmentOptions.Center;
			bodyLabel.enableWordWrapping = true;
			bodyLabel.overflowMode = TextOverflowModes.Ellipsis;
			bodyLabel.color = new Color(0.96f, 0.96f, 0.98f, 0.98f);

			var footer = CreateText("CampaignStatusFooter", _statusToastOverlay, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(304f, 18f), 10.5f, "Campaign message");
			footer.alignment = TextAlignmentOptions.Center;
			footer.color = new Color(0.84f, 0.84f, 0.88f, 0.78f);

			_statusToastOverlay.SetAsLastSibling();
			AutoHideStatusToast(currentVersion).Forget();
		}

		private async UniTaskVoid AutoHideClaimToast(int version)
		{
			await UniTask.Delay(TimeSpan.FromSeconds(3.2));
			if (version != _claimToastVersion)
				return;

			HideClaimToast();
		}

		private void HideClaimToast()
		{
			if (!_claimToastOverlay)
				return;

			Destroy(_claimToastOverlay.gameObject);
			_claimToastOverlay = null;
		}

		private async UniTaskVoid AutoHideTreasureToast(int version)
		{
			await UniTask.Delay(TimeSpan.FromSeconds(3.4));
			if (version != _treasureToastVersion)
				return;

			HideTreasureToast();
		}

		private async UniTaskVoid AutoHideStatusToast(int version)
		{
			await UniTask.Delay(TimeSpan.FromSeconds(4));
			if (version != _statusToastVersion)
				return;

			HideStatusToast();
		}

		private void HideTreasureToast()
		{
			if (!_treasureToastOverlay)
				return;

			Destroy(_treasureToastOverlay.gameObject);
			_treasureToastOverlay = null;
		}

		private void HideStatusToast()
		{
			if (!_statusToastOverlay)
				return;

			Destroy(_statusToastOverlay.gameObject);
			_statusToastOverlay = null;
		}

		private void CreateMilestoneRow(Transform parent, CampaignMilestoneRewardModel milestone, int index)
		{
			var row = CreateRect($"Milestone_{milestone.Id}", parent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -52f - index * 48f), new Vector2(410f, 40f));
			var rowImage = row.gameObject.AddComponent<Image>();
			rowImage.color = milestone.IsClaimed
				? new Color(0.08f, 0.18f, 0.1f, 0.94f)
				: milestone.IsAchieved
					? new Color(0.13f, 0.2f, 0.12f, 0.94f)
					: new Color(0.1f, 0.1f, 0.12f, 0.92f);
			var rowOutline = row.gameObject.AddComponent<Outline>();
			rowOutline.effectColor = milestone.IsClaimed
				? new Color(0.44f, 0.88f, 0.38f, 0.72f)
				: milestone.IsAchieved
					? new Color(0.95f, 0.75f, 0.2f, 0.62f)
					: new Color(0.95f, 0.75f, 0.2f, 0.28f);
			rowOutline.effectDistance = new Vector2(1f, -1f);
			rowOutline.useGraphicAlpha = true;

			var statusText = milestone.IsClaimed ? "CLAIMED" : milestone.IsClaimable ? "READY" : milestone.IsAchieved ? "ACHIEVED" : "PENDING";
			var status = CreateText("MilestoneStatus", row, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(12f, 0f), new Vector2(92f, 18f), 11f, statusText);
			status.alignment = TextAlignmentOptions.Left;
			status.fontStyle = FontStyles.Bold;
			status.color = milestone.IsClaimed
				? new Color(0.6f, 1f, 0.56f, 0.98f)
				: milestone.IsClaimable
					? new Color(1f, 0.9f, 0.5f, 0.98f)
					: milestone.IsAchieved
						? new Color(0.9f, 0.98f, 0.72f, 0.96f)
						: new Color(0.98f, 0.84f, 0.38f, 0.92f);

			var title = CreateText("MilestoneTitle", row, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(104f, 8f), new Vector2(212f, 16f), 13f, milestone.Title.ToUpperInvariant());
			title.alignment = TextAlignmentOptions.Left;
			title.fontStyle = FontStyles.Bold;
			title.color = Color.white;

			var description = CreateText("MilestoneDescription", row, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(104f, -9f), new Vector2(260f, 14f), 10.5f, milestone.Description);
			description.alignment = TextAlignmentOptions.Left;
			description.color = new Color(0.9f, 0.9f, 0.92f, 0.92f);
			description.enableWordWrapping = false;
			description.overflowMode = TextOverflowModes.Ellipsis;

			if (!milestone.IsClaimable && !milestone.IsClaimed)
				return;

			var claimButtonRect = CreateRect("ClaimMilestoneButton", row, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-12f, 0f), new Vector2(84f, 24f));
			var claimImage = claimButtonRect.gameObject.AddComponent<Image>();
			claimImage.color = milestone.IsClaimed
				? new Color(0.2f, 0.34f, 0.2f, 0.94f)
				: new Color(1f, 0.78f, 0.2f, 0.96f);
			var claimButton = claimButtonRect.gameObject.AddComponent<Button>();
			claimButton.targetGraphic = claimImage;
			claimButton.interactable = milestone.IsClaimable;
			if (milestone.IsClaimable)
				claimButton.onClick.AddListener(() => ClaimMilestoneAsync(milestone).Forget());

			var claimLabel = CreateText("ClaimMilestoneLabel", claimButtonRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(70f, 18f), 10.5f, milestone.IsClaimed ? "CLAIMED" : "CLAIM");
			claimLabel.alignment = TextAlignmentOptions.Center;
			claimLabel.fontStyle = FontStyles.Bold;
			claimLabel.color = milestone.IsClaimed ? new Color(0.9f, 1f, 0.9f, 0.98f) : new Color(0.18f, 0.1f, 0.02f, 1f);
		}

		private void CreateTreasureSummaryRow(Transform parent, CampaignTreasureChoiceModel treasure, int index)
		{
			var row = CreateRect($"TreasureSummary_{treasure.Id}", parent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -52f - index * 42f), new Vector2(296f, 34f));
			var rowImage = row.gameObject.AddComponent<Image>();
			rowImage.color = new Color(0.14f, 0.1f, 0.06f, 0.94f);
			var title = CreateText("TreasureSummaryTitle", row, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(10f, 8f), new Vector2(170f, 14f), 11.5f, treasure.Title.ToUpperInvariant());
			title.alignment = TextAlignmentOptions.Left;
			title.fontStyle = FontStyles.Bold;
			title.color = new Color(0.98f, 0.84f, 0.38f, 1f);

			var bonus = CreateText("TreasureSummaryBonus", row, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(10f, -8f), new Vector2(260f, 14f), 10.5f, BuildTreasureBonusText(treasure));
			bonus.alignment = TextAlignmentOptions.Left;
			bonus.color = new Color(0.94f, 0.94f, 0.96f, 0.92f);
		}

		private void CreateQuadrantSummaryRow(Transform parent, CampaignQuadrantProgressModel quadrant, int index)
		{
			var row = CreateRect($"QuadrantSummary_{quadrant.Quadrant}", parent, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 126f - index * 44f), new Vector2(296f, 38f));
			var rowImage = row.gameObject.AddComponent<Image>();
			rowImage.color = quadrant.IsCompleted ? new Color(0.12f, 0.19f, 0.12f, 0.92f) : new Color(0.1f, 0.1f, 0.12f, 0.9f);

			var title = CreateText("QuadrantSummaryTitle", row, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(10f, 8f), new Vector2(132f, 14f), 11.5f, quadrant.DisplayName.ToUpperInvariant());
			title.alignment = TextAlignmentOptions.Left;
			title.fontStyle = FontStyles.Bold;
			title.color = Color.white;

			var progress = CreateText("QuadrantSummaryProgress", row, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-10f, 8f), new Vector2(136f, 14f), 10f, quadrant.ProgressLabel.ToUpperInvariant());
			progress.alignment = TextAlignmentOptions.Right;
			progress.fontStyle = FontStyles.Bold;
			progress.color = quadrant.IsCompleted ? new Color(0.6f, 1f, 0.56f, 0.96f) : new Color(0.98f, 0.84f, 0.38f, 0.92f);

			var summary = CreateText("QuadrantSummaryBody", row, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(10f, -9f), new Vector2(268f, 14f), 10f, quadrant.IsFinaleQuadrant ? quadrant.RewardSummary : quadrant.HeroicRewardSummary);
			summary.alignment = TextAlignmentOptions.Left;
			summary.color = new Color(0.9f, 0.9f, 0.92f, 0.9f);
			summary.enableWordWrapping = false;
			summary.overflowMode = TextOverflowModes.Ellipsis;
		}

		private string BuildUnlocksSummaryText()
		{
			var titles = _campaignProgress.UnlockedTitles?.Count > 0
				? string.Join(", ", _campaignProgress.UnlockedTitles)
				: "No title rewards claimed yet";
			var cosmetics = _campaignProgress.UnlockedCosmetics?.Count > 0
				? string.Join(", ", _campaignProgress.UnlockedCosmetics)
				: "No cosmetic rewards claimed yet";
			return $"Titles: {titles}\nCosmetics: {cosmetics}";
		}

		private static string BuildMilestoneClaimRewardText(CampaignMilestoneRewardModel milestone)
		{
			return milestone?.Id switch
			{
				"quadrant_1" => "+500 BXP reserved\n+3 faction-card rewards reserved",
				"quadrant_2" => "+1000 BXP reserved\n+5 faction-card rewards reserved",
				"quadrant_3" => "+2000 BXP reserved\n+1 rare-card placeholder reserved",
				"quadrant_4" => "+5000 BXP reserved\nVulcan City progression secured",
				"vulcan_city" => "Title placeholder unlocked:\nForgebreaker",
				"three_star_mastery" => "Cosmetic placeholder unlocked:\nQuadrant Trials Mastery Aura",
				_ => "Campaign placeholder reward recorded."
			};
		}

		private void CreateGuideSection(Transform parent, string name, Vector2 anchoredPosition, string title, string body)
		{
			var card = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, new Vector2(350f, 94f));
			var cardImage = card.gameObject.AddComponent<Image>();
			cardImage.color = new Color(0.08f, 0.09f, 0.12f, 0.92f);
			var outline = card.gameObject.AddComponent<Outline>();
			outline.effectColor = new Color(0.95f, 0.75f, 0.2f, 0.44f);
			outline.effectDistance = new Vector2(1f, -1f);
			outline.useGraphicAlpha = true;

			var cardTitle = CreateText($"{name}Title", card, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -12f), new Vector2(220f, 22f), 17f, title);
			cardTitle.fontStyle = FontStyles.Bold;
			cardTitle.color = new Color(0.98f, 0.84f, 0.38f, 1f);
			cardTitle.alignment = TextAlignmentOptions.TopLeft;

			var cardBody = CreateText($"{name}Body", card, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, -38f), new Vector2(316f, 46f), 13.5f, body);
			cardBody.color = new Color(0.95f, 0.95f, 0.97f, 0.96f);
			cardBody.alignment = TextAlignmentOptions.TopLeft;
			cardBody.enableWordWrapping = true;
			cardBody.overflowMode = TextOverflowModes.Ellipsis;
			//proper stretching
			cardBody.rectTransform.offsetMin = new Vector2(16f, cardBody.rectTransform.offsetMin.y);
			cardBody.rectTransform.offsetMax = new Vector2(-16f, cardBody.rectTransform.offsetMax.y);
		}

		private async UniTaskVoid LoadCampaignProgressAsync()
		{
			try
			{
				var response = await CampaignAPI.GetProgress().AsUniTask().AddLoadingTask();
				if (!response)
				{
					ShowCampaignStatus("CAMPAIGN LOAD FAILED", GetResponseMessage(response), true);
					return;
				}

				_campaignProgress = response.Data;

				if (_pendingCampaignResult.HasValue)
				{
					var pendingResult = _pendingCampaignResult.Value;
					_isHeroicMode = pendingResult.IsHeroic;
					var targetQuadrant = CampaignModeSettings.Quadrants.FirstOrDefault(x => x.Quadrant == pendingResult.Quadrant);
					if (targetQuadrant != null)
						ShowStageMap(targetQuadrant);
					else
						ShowQuadrantSelection();

					ShowBattleResultsOverlay(pendingResult);
					return;
				}

				if (_currentQuadrant != null)
				{
					ShowStageMap(_currentQuadrant);
					if (_campaignProgress?.PendingTreasureChoices?.Count > 0)
						ShowTreasureSelection();
					return;
				}

				ShowQuadrantSelection();
				if (_campaignProgress?.PendingTreasureChoices?.Count > 0)
					ShowTreasureSelection();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				ShowCampaignStatus("CAMPAIGN LOAD FAILED", "Could not load campaign progress.", true);
			}
		}

		private async UniTaskVoid HandleStageSelectedAsync(CampaignQuadrantDefinition quadrant, CampaignStageDefinition stage, CampaignNodeState state)
		{
			try
			{
				UpdateStageDetails(quadrant, stage, state);
				if (state == CampaignNodeState.Locked)
				{
					var quadrantProgress = GetQuadrantProgress(quadrant.Quadrant);
					if (_isHeroicMode && quadrantProgress != null && !quadrantProgress.IsHeroicUnlocked)
						ShowCampaignStatus("HEROIC LOCKED", "Clear every Normal encounter in this quadrant to unlock Heroic mode.");
					else
						ShowCampaignStatus("NODE LOCKED", "Clear the current encounter to unlock this node.");
					return;
				}

				var decks = DeckApplicationAdapter.Application?.All?.ToArray() ?? Array.Empty<DeckData>();
				if (decks.Length == 0)
				{
					ShowCampaignStatus("NO DECK SELECTED", "Create or select a deck before starting Campaign.");
					return;
				}

				if (decks.Length == 1)
				{
					await StartCampaignStageAsync(quadrant, stage, decks[0]);
					return;
				}

				ShowDeckSelection(quadrant, stage, decks);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				ShowCampaignStatus("STAGE START FAILED", "Could not start campaign stage.", true);
			}
		}

		private async UniTask StartCampaignStageAsync(CampaignQuadrantDefinition quadrant, CampaignStageDefinition stage, DeckData deck)
		{
			if (deck == null || string.IsNullOrWhiteSpace(deck.Id))
			{
				ShowCampaignStatus("INVALID DECK", "Select a valid deck before starting Campaign.");
				return;
			}

			if (DeckApplicationAdapter.Application?.Current?.Id != deck.Id)
				await DeckApplicationAdapter.Application.SelectAsync(deck).AddLoadingTask();

			var response = await CampaignAPI.PostStartStage(new CampaignStartStageModel
			{
				Quadrant = quadrant.Quadrant,
				StageIndex = stage.Index,
				DeckId = deck.Id,
				IsHeroic = _isHeroicMode
			}).AsUniTask().AddLoadingTask();

			if (!response)
			{
				ShowCampaignStatus("STAGE START FAILED", GetResponseMessage(response), true);
				return;
			}

			if (PracticeApplicationAdapter.Application == null || response.Data == null)
			{
				ShowCampaignStatus("JOIN FAILED", "Campaign stage started, but the client could not join it.", true);
				return;
			}

			HideDeckSelection();
			User.AddRedirection(new CampaignRedirectArg(quadrant.Quadrant, stage.Index, stage.Title, _isHeroicMode));
			ArenaThemeSettings.SetRuntimeOverride(GetCampaignArenaTheme(quadrant.Quadrant));
			BoardLayoutSettings.ClearRuntimeOverride();
			Close();

			if (!await PracticeApplicationAdapter.Application.JoinGameAsync(response.Data))
				ShowCampaignStatus("JOIN FAILED", "Campaign stage started, but the client could not join it.", true);
		}

		private void ShowDeckSelection(CampaignQuadrantDefinition quadrant, CampaignStageDefinition stage, DeckData[] decks)
		{
			HideDeckSelection();

			_deckSelectionOverlay = CreateRect("CampaignDeckSelection", _panelRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 420f));
			var overlayImage = _deckSelectionOverlay.gameObject.AddComponent<Image>();
			overlayImage.color = new Color(0.02f, 0.03f, 0.04f, 0.95f);
			var overlayOutline = _deckSelectionOverlay.gameObject.AddComponent<Outline>();
			overlayOutline.effectColor = GetQuadrantColor(quadrant.Id);
			overlayOutline.effectDistance = new Vector2(2f, -2f);
			overlayOutline.useGraphicAlpha = true;

			var title = CreateText("DeckSelectionTitle", _deckSelectionOverlay, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(620f, 30f), 24f, $"Choose a deck for {stage.Title}");
			title.fontStyle = FontStyles.Bold;
			title.color = new Color(0.98f, 0.84f, 0.38f, 1f);
			title.alignment = TextAlignmentOptions.Center;

			var subtitle = CreateText("DeckSelectionSubtitle", _deckSelectionOverlay, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -58f), new Vector2(640f, 20f), 13f, "Campaign uses your current collection. Pick the deck you want to take into this encounter.");
			subtitle.alignment = TextAlignmentOptions.Center;
			subtitle.color = new Color(0.9f, 0.9f, 0.92f, 0.92f);

			var deckList = CreateRect("DeckList", _deckSelectionOverlay, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 8f), new Vector2(680f, 228f));

			for (var i = 0; i < decks.Length; i++)
			{
				var deck = decks[i];
				var topRow = i < 3;
				var column = topRow ? i : i - 3;
				var x = decks.Length <= 3
					? (i - (decks.Length - 1) * 0.5f) * 220f
					: -220f + column * 220f;
				var y = decks.Length <= 3 ? 0f : (topRow ? 58f : -58f);
				CreateDeckSelectionCard(deckList, deck, quadrant, stage, new Vector2(x, y));
			}

			var cancelGo = Instantiate(_generalButtonPrefab, _deckSelectionOverlay, false);
			cancelGo.name = "CancelButton";
			var rect = cancelGo.GetComponent<RectTransform>();
			rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
			rect.pivot = new Vector2(0.5f, 0f);
			rect.anchoredPosition = new Vector2(0f, 18f);
			rect.sizeDelta = new Vector2(240f, 58f);

			var cancel = cancelGo.GetComponent<Button>();
			cancel.onClick.AddListener(HideDeckSelection);

			var cancelLabel = cancelGo.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
			if (cancelLabel != null)
				cancelLabel.text = "CANCEL";

			_deckSelectionOverlay.SetAsLastSibling();
		}

		private void CreateDeckSelectionCard(Transform parent, DeckData deck, CampaignQuadrantDefinition quadrant, CampaignStageDefinition stage, Vector2 anchoredPosition)
		{
			var card = CreateRect($"Deck_{deck.Id}", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, new Vector2(196f, 100f));
			var cardImage = card.gameObject.AddComponent<Image>();
			cardImage.color = new Color(0.1f, 0.1f, 0.12f, 0.96f);
			var outline = card.gameObject.AddComponent<Outline>();
			outline.effectColor = DeckApplicationAdapter.Application?.Current?.Id == deck.Id
				? new Color(1f, 0.82f, 0.24f, 0.9f)
				: new Color(0.42f, 0.42f, 0.46f, 0.72f);
			outline.effectDistance = new Vector2(1.5f, -1.5f);
			outline.useGraphicAlpha = true;

			var deckName = CreateText("DeckName", card, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(160f, 24f), 18f, deck.Name?.ToUpperInvariant() ?? "UNNAMED DECK");
			deckName.fontStyle = FontStyles.Bold;
			deckName.alignment = TextAlignmentOptions.Center;
			deckName.color = new Color(0.98f, 0.84f, 0.38f, 1f);
			deckName.enableAutoSizing = true;
			deckName.fontSizeMin = 13f;
			deckName.fontSizeMax = 18f;
			deckName.enableWordWrapping = false;
			deckName.overflowMode = TextOverflowModes.Ellipsis;

			var deckMeta = CreateText("DeckMeta", card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -2f), new Vector2(160f, 18f), 11f, $"{deck.OwnedCardIds?.Count ?? 0} cards");
			deckMeta.alignment = TextAlignmentOptions.Center;
			deckMeta.color = new Color(0.88f, 0.88f, 0.9f, 0.9f);

			var status = CreateText("DeckStatus", card, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(156f, 16f), 10f, DeckApplicationAdapter.Application?.Current?.Id == deck.Id ? "CURRENTLY SELECTED" : "AVAILABLE");
			status.alignment = TextAlignmentOptions.Center;
			status.fontStyle = FontStyles.Bold;
			status.color = DeckApplicationAdapter.Application?.Current?.Id == deck.Id
				? new Color(1f, 0.82f, 0.24f, 0.96f)
				: new Color(0.86f, 0.86f, 0.9f, 0.82f);

			var chooseButtonRect = CreateRect("ChooseButton", card, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(132f, 30f));
			var chooseButtonImage = chooseButtonRect.gameObject.AddComponent<Image>();
			chooseButtonImage.color = new Color(1f, 0.78f, 0.2f, 0.96f);
			var chooseButton = chooseButtonRect.gameObject.AddComponent<Button>();
			chooseButton.targetGraphic = chooseButtonImage;
			chooseButton.onClick.AddListener(() => SelectCampaignDeckAndStartAsync(quadrant, stage, deck).Forget());
			var chooseLabel = CreateText("ChooseLabel", chooseButtonRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(110f, 18f), 11f, "USE THIS DECK");
			chooseLabel.alignment = TextAlignmentOptions.Center;
			chooseLabel.fontStyle = FontStyles.Bold;
			chooseLabel.color = new Color(0.16f, 0.1f, 0.02f, 1f);
		}

		private async UniTaskVoid SelectCampaignDeckAndStartAsync(CampaignQuadrantDefinition quadrant, CampaignStageDefinition stage, DeckData deck)
		{
			try
			{
				await StartCampaignStageAsync(quadrant, stage, deck);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				ShowCampaignStatus("STAGE START FAILED", "Could not start campaign stage.", true);
			}
		}

		private void HideDeckSelection()
		{
			if (!_deckSelectionOverlay)
				return;

			Destroy(_deckSelectionOverlay.gameObject);
			_deckSelectionOverlay = null;
		}

		private void ShowTreasureSelection()
		{
			HideTreasureSelection();
			var treasures = _campaignProgress?.PendingTreasureChoices;
			if (treasures == null || treasures.Count == 0)
				return;

			_treasureSelectionOverlay = CreateRect("CampaignTreasureSelection", _panelRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(780f, 430f));
			var overlayImage = _treasureSelectionOverlay.gameObject.AddComponent<Image>();
			overlayImage.color = new Color(0.02f, 0.025f, 0.035f, 0.97f);
			var overlayOutline = _treasureSelectionOverlay.gameObject.AddComponent<Outline>();
			overlayOutline.effectColor = new Color(1f, 0.62f, 0.14f, 0.88f);
			overlayOutline.effectDistance = new Vector2(2f, -2f);
			overlayOutline.useGraphicAlpha = true;

			var title = CreateText("TreasureSelectionTitle", _treasureSelectionOverlay, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(640f, 30f), 26f, "CHOOSE A TREASURE");
			title.fontStyle = FontStyles.Bold;
			title.alignment = TextAlignmentOptions.Center;
			title.color = new Color(0.98f, 0.84f, 0.38f, 1f);

			var subtitle = CreateText("TreasureSelectionSubtitle", _treasureSelectionOverlay, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -58f), new Vector2(680f, 22f), 14f, "Pick one persistent campaign reward before your next stage.");
			subtitle.alignment = TextAlignmentOptions.Center;
			subtitle.color = new Color(0.9f, 0.9f, 0.92f, 0.92f);

			var list = CreateRect("TreasureList", _treasureSelectionOverlay, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 6f), new Vector2(700f, 240f));
			for (var i = 0; i < treasures.Count; i++)
			{
				var x = (i - (treasures.Count - 1) * 0.5f) * 228f;
				CreateTreasureChoiceCard(list, treasures[i], new Vector2(x, 0f));
			}

			var footer = CreateText("TreasureSelectionFooter", _treasureSelectionOverlay, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(660f, 18f), 11f, "Treasure bonuses apply to your future campaign battles.");
			footer.alignment = TextAlignmentOptions.Center;
			footer.color = new Color(0.86f, 0.86f, 0.9f, 0.8f);

			_treasureSelectionOverlay.SetAsLastSibling();
		}

		private void CreateTreasureChoiceCard(Transform parent, CampaignTreasureChoiceModel treasure, Vector2 anchoredPosition)
		{
			var card = CreateRect($"Treasure_{treasure.Id}", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, new Vector2(204f, 188f));
			var cardImage = card.gameObject.AddComponent<Image>();
			cardImage.color = new Color(0.11f, 0.09f, 0.06f, 0.96f);
			var outline = card.gameObject.AddComponent<Outline>();
			outline.effectColor = new Color(1f, 0.64f, 0.16f, 0.84f);
			outline.effectDistance = new Vector2(2f, -2f);
			outline.useGraphicAlpha = true;

			var title = CreateText("TreasureTitle", card, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(172f, 28f), 20f, treasure.Title.ToUpperInvariant());
			title.fontStyle = FontStyles.Bold;
			title.alignment = TextAlignmentOptions.Center;
			title.color = new Color(0.98f, 0.84f, 0.38f, 1f);

			var bonus = CreateText("TreasureBonus", card, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(170f, 22f), 12f, BuildTreasureBonusText(treasure));
			bonus.alignment = TextAlignmentOptions.Center;
			bonus.fontStyle = FontStyles.Bold;
			bonus.color = new Color(1f, 0.9f, 0.58f, 0.96f);

			var description = CreateText("TreasureDescription", card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -6f), new Vector2(164f, 58f), 15f, treasure.Description);
			description.alignment = TextAlignmentOptions.Center;
			description.color = new Color(0.96f, 0.96f, 0.98f, 0.98f);
			description.enableWordWrapping = true;
			description.overflowMode = TextOverflowModes.Ellipsis;

			var chooseButtonRect = CreateRect("ChooseTreasureButton", card, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(138f, 32f));
			var chooseButtonImage = chooseButtonRect.gameObject.AddComponent<Image>();
			chooseButtonImage.color = new Color(1f, 0.78f, 0.2f, 0.98f);
			var chooseButton = chooseButtonRect.gameObject.AddComponent<Button>();
			chooseButton.targetGraphic = chooseButtonImage;
			chooseButton.onClick.AddListener(() => SelectTreasureAsync(treasure).Forget());
			var chooseLabel = CreateText("ChooseTreasureLabel", chooseButtonRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(120f, 18f), 12f, "TAKE TREASURE");
			chooseLabel.alignment = TextAlignmentOptions.Center;
			chooseLabel.fontStyle = FontStyles.Bold;
			chooseLabel.color = new Color(0.16f, 0.1f, 0.02f, 1f);
		}

		private async UniTaskVoid SelectTreasureAsync(CampaignTreasureChoiceModel treasure)
		{
			try
			{
				var response = await CampaignAPI.PostChooseTreasure(new CampaignChooseTreasureModel
				{
					TreasureId = treasure.Id
				}).AsUniTask().AddLoadingTask();

				if (!response)
				{
					ShowCampaignStatus("TREASURE CLAIM FAILED", GetResponseMessage(response), true);
					return;
				}

				_campaignProgress = response.Data;
				HideTreasureSelection();
				if (_currentQuadrant != null)
					ShowStageMap(_currentQuadrant);
				else
					ShowQuadrantSelection();
				ShowTreasureClaimToast(treasure);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				ShowCampaignStatus("TREASURE CLAIM FAILED", "Could not claim campaign treasure.", true);
			}
		}

		private async UniTaskVoid ClaimMilestoneAsync(CampaignMilestoneRewardModel milestone)
		{
			if (milestone == null || !milestone.IsClaimable)
				return;

			try
			{
				var response = await CampaignAPI.PostClaimMilestone(new CampaignClaimMilestoneModel
				{
					MilestoneId = milestone.Id
				}).AsUniTask().AddLoadingTask();

				if (!response)
				{
					ShowCampaignStatus("MILESTONE CLAIM FAILED", GetResponseMessage(response), true);
					return;
				}

				_campaignProgress = response.Data;
				HideRewardsOverlay();
				ShowRewardsOverlay();
				ShowMilestoneClaimToast(milestone);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				ShowCampaignStatus("MILESTONE CLAIM FAILED", "Could not claim campaign milestone.", true);
			}
		}

		private void HideTreasureSelection()
		{
			if (!_treasureSelectionOverlay)
				return;

			Destroy(_treasureSelectionOverlay.gameObject);
			_treasureSelectionOverlay = null;
		}

		private void ShowBattleResultsOverlay(CampaignRedirectArg result)
		{
			HideBattleResultsOverlay();

			var quadrant = CampaignModeSettings.Quadrants.FirstOrDefault(x => x.Quadrant == result.Quadrant);
			if (quadrant == null)
				return;

			var quadrantProgress = GetQuadrantProgress(result.Quadrant);
			var stageProgress = GetDisplayedStageProgresses(quadrantProgress, result.IsHeroic)?.FirstOrDefault(x => x.StageIndex == result.StageIndex);
			var stageDefinition = quadrant.Stages.FirstOrDefault(x => x.Index == result.StageIndex);
			_pendingCampaignResult = null;

			_battleResultsOverlay = CreateRect("CampaignBattleResults", _panelRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(768f, 452f));
			var overlayImage = _battleResultsOverlay.gameObject.AddComponent<Image>();
			overlayImage.color = new Color(0.02f, 0.03f, 0.04f, 0.975f);
			var overlayOutline = _battleResultsOverlay.gameObject.AddComponent<Outline>();
			overlayOutline.effectColor = result.DidWin ? new Color(0.95f, 0.75f, 0.2f, 0.88f) : new Color(0.94f, 0.28f, 0.18f, 0.82f);
			overlayOutline.effectDistance = new Vector2(2f, -2f);
			overlayOutline.useGraphicAlpha = true;

			var title = CreateText("ResultsTitle", _battleResultsOverlay, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(520f, 28f), 28f, result.DidWin ? "CAMPAIGN VICTORY" : "CAMPAIGN DEFEAT");
			title.alignment = TextAlignmentOptions.Center;
			title.fontStyle = FontStyles.Bold;
			title.color = result.DidWin ? new Color(0.98f, 0.84f, 0.38f, 1f) : new Color(1f, 0.65f, 0.56f, 0.98f);

			var stageTitle = CreateText("ResultsStageTitle", _battleResultsOverlay, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -58f), new Vector2(560f, 22f), 18f, (result.StageTitle ?? stageDefinition?.Title ?? quadrant.DisplayName).ToUpperInvariant());
			stageTitle.alignment = TextAlignmentOptions.Center;
			stageTitle.fontStyle = FontStyles.Bold;
			stageTitle.color = new Color(0.94f, 0.94f, 0.96f, 0.98f);

			var artFrame = CreateRect("ResultsArtFrame", _battleResultsOverlay, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(22f, -94f), new Vector2(196f, 116f));
			var artFrameImage = artFrame.gameObject.AddComponent<Image>();
			artFrameImage.color = new Color(0f, 0f, 0f, 0.42f);
			var artGo = new GameObject("ResultsArt", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
			artGo.transform.SetParent(artFrame, false);
			var artRect = artGo.GetComponent<RectTransform>();
			artRect.anchorMin = Vector2.zero;
			artRect.anchorMax = Vector2.one;
			artRect.offsetMin = new Vector2(2f, 2f);
			artRect.offsetMax = new Vector2(-2f, -2f);
			var artImage = artGo.GetComponent<RawImage>();
			artImage.texture = Resources.Load<Texture2D>(quadrant.PreviewTextureResource);
			artImage.color = Color.white;

			var starsTitle = CreateText("ResultsStarsTitle", _battleResultsOverlay, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(244f, -106f), new Vector2(240f, 20f), 14f, "STARS EARNED");
			starsTitle.fontStyle = FontStyles.Bold;
			starsTitle.color = new Color(0.98f, 0.84f, 0.38f, 1f);
			CreateResultsStars(_battleResultsOverlay, stageProgress?.Stars ?? 0, new Vector2(246f, -136f));

			var summary = CreateText("ResultsSummary", _battleResultsOverlay, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(244f, -162f), new Vector2(486f, 58f), 18f,
				BuildCampaignResultSummary(result, quadrant, quadrantProgress, stageProgress));
			summary.color = new Color(0.96f, 0.96f, 0.98f, 0.98f);
			summary.fontStyle = FontStyles.Bold;
			summary.alignment = TextAlignmentOptions.TopLeft;
			summary.enableWordWrapping = true;

			var rewardsPanel = CreateRect("ResultsRewardsPanel", _battleResultsOverlay, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(720f, 98f));
			var rewardsPanelImage = rewardsPanel.gameObject.AddComponent<Image>();
			rewardsPanelImage.color = new Color(0.08f, 0.09f, 0.11f, 0.94f);

			var rewardsTitle = CreateText("ResultsRewardsTitle", rewardsPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -14f), new Vector2(320f, 20f), 16f, "BATTLE PAYOUT");
			rewardsTitle.fontStyle = FontStyles.Bold;
			rewardsTitle.color = new Color(0.98f, 0.84f, 0.38f, 1f);

			var rewardsBody = CreateText("ResultsRewardsBody", rewardsPanel, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(16f, -38f), new Vector2(688f, 44f), 15f,
				result.DidWin
					? $"{stageProgress?.StarRewardText ?? "Stage rewards recorded."}\n{BuildCampaignNextStep(result, quadrantProgress, stageProgress)}"
					: BuildCampaignNextStep(result, quadrantProgress, stageProgress));
			rewardsBody.color = new Color(0.96f, 0.96f, 0.98f, 0.96f);
			rewardsBody.enableWordWrapping = true;

			if (result.DidWin && _campaignProgress?.PendingTreasureChoices?.Count > 0)
			{
				var treasurePanel = CreateRect("ResultsTreasurePanel", _battleResultsOverlay, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 88f), new Vector2(720f, 82f));
				var treasurePanelImage = treasurePanel.gameObject.AddComponent<Image>();
				treasurePanelImage.color = new Color(0.13f, 0.08f, 0.03f, 0.95f);
				var treasureTitle = CreateText("ResultsTreasureTitle", treasurePanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -12f), new Vector2(260f, 20f), 15f, "TREASURE DISCOVERED");
				treasureTitle.fontStyle = FontStyles.Bold;
				treasureTitle.color = new Color(1f, 0.9f, 0.56f, 0.98f);

				var treasurePreview = string.Join("  •  ", _campaignProgress.PendingTreasureChoices.Select(x => x.Title.ToUpperInvariant()));
				var treasureBody = CreateText("ResultsTreasureBody", treasurePanel, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(16f, -36f), new Vector2(686f, 30f), 14f,
					$"Choose 1 reward before the next stage: {treasurePreview}");
				treasureBody.color = new Color(0.98f, 0.98f, 0.98f, 0.98f);
				treasureBody.enableWordWrapping = true;
			}

			var continueGo = Instantiate(_generalButtonPrefab, _battleResultsOverlay, false);
			continueGo.name = "ResultsContinueButton";
			var rect = continueGo.GetComponent<RectTransform>();
			rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
			rect.pivot = new Vector2(0.5f, 0f);
			rect.anchoredPosition = new Vector2(0f, 14f);
			rect.sizeDelta = new Vector2(220, 26);

			var continuePress = continueGo.GetComponent<Button>();
			continuePress.onClick.AddListener(() =>
			{
				HideBattleResultsOverlay();
				if (_campaignProgress?.PendingTreasureChoices?.Count > 0 && result.DidWin)
					ShowTreasureSelection();
			});

			var continueLabel = continueGo.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
			if (continueLabel != null)
			{
				continueLabel.fontSize = 18f;
				continueLabel.color = new Color(0.95f, 0.85f, 0.6f, 1f);
				continueLabel.fontStyle = FontStyles.Bold;
				continueLabel.text = result.DidWin && _campaignProgress?.PendingTreasureChoices?.Count > 0 ? "CHOOSE TREASURE" : "CONTINUE CAMPAIGN";
			}

			_battleResultsOverlay.SetAsLastSibling();
		}

		private void CreateBtn()
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
			buttonGo.transform.SetAsLastSibling();
		}

		private void HideBattleResultsOverlay()
		{
			if (!_battleResultsOverlay)
				return;

			Destroy(_battleResultsOverlay.gameObject);
			_battleResultsOverlay = null;
		}

		private CampaignQuadrantProgressModel GetQuadrantProgress(Quadrant quadrant)
		{
			return _campaignProgress?.Quadrants?.FirstOrDefault(x => x.Quadrant == quadrant);
		}

		private CampaignStageProgressModel GetStageProgress(CampaignQuadrantDefinition quadrant, int stageIndex)
		{
			return GetDisplayedStageProgresses(GetQuadrantProgress(quadrant.Quadrant))?.FirstOrDefault(x => x.StageIndex == stageIndex);
		}

		private CampaignStageProgressModel GetStageProgress(Quadrant quadrant, int stageIndex, bool isHeroic)
		{
			return GetDisplayedStageProgresses(GetQuadrantProgress(quadrant), isHeroic)?.FirstOrDefault(x => x.StageIndex == stageIndex);
		}

		private CampaignStageDefinition GetInitialStageDefinition(CampaignQuadrantDefinition quadrant)
		{
			var currentStage = quadrant.Stages.FirstOrDefault(stage => GetStageNodeState(stage) == CampaignNodeState.Current);
			return currentStage ?? quadrant.Stages.FirstOrDefault();
		}

		private void CreateResultsStars(Transform parent, int stars, Vector2 anchoredPosition)
		{
			var row = CreateRect("ResultsStars", parent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), anchoredPosition, new Vector2(116f, 18f));
			for (var i = 0; i < 3; i++)
			{
				var pip = CreateRect($"ResultStar_{i}", row, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(i * 28f, 0f), new Vector2(20f, 20f));
				var pipImage = pip.gameObject.AddComponent<Image>();
				pipImage.color = i < Mathf.Clamp(stars, 0, 3)
					? new Color(1f, 0.82f, 0.24f, 0.98f)
					: new Color(0.34f, 0.34f, 0.38f, 0.78f);
				var pipOutline = pip.gameObject.AddComponent<Outline>();
				pipOutline.effectColor = i < Mathf.Clamp(stars, 0, 3)
					? new Color(1f, 0.95f, 0.62f, 0.78f)
					: new Color(0f, 0f, 0f, 0.34f);
				pipOutline.effectDistance = new Vector2(1f, -1f);
				pipOutline.useGraphicAlpha = true;
			}
		}

		private CampaignNodeState GetStageNodeState(CampaignStageDefinition stage)
		{
			if (_currentQuadrant == null)
				return stage.Index == 1 ? CampaignNodeState.Current : CampaignNodeState.Locked;

			var progress = GetStageProgress(_currentQuadrant, stage.Index);
			if (progress == null)
				return stage.Index == 1 ? CampaignNodeState.Current : CampaignNodeState.Locked;
			if (progress.IsCompleted)
				return CampaignNodeState.Cleared;
			if (progress.IsUnlocked)
				return CampaignNodeState.Current;
			return CampaignNodeState.Locked;
		}

		private static string GetCampaignArenaTheme(Quadrant quadrant)
		{
			return quadrant switch
			{
				Quadrant.Boreas => ArenaThemeSettings.Boreas,
				Quadrant.Arcadia => ArenaThemeSettings.Arcadia,
				Quadrant.Notus => ArenaThemeSettings.Notus,
				Quadrant.Hades => ArenaThemeSettings.Hades,
				Quadrant.Vulcan_City => ArenaThemeSettings.Colosseum,
				_ => ArenaThemeSettings.GetSelectedValue()
			};
		}

		private static string GetResponseMessage<T>(RR.Network.Rest.APIResponse<T> response) where T : class
		{
			if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
				return response.ErrorMessage;
			if (!string.IsNullOrWhiteSpace(response.StatusCodeMessage))
				return response.StatusCodeMessage;
			return "Campaign request failed.";
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
			label.enableAutoSizing = false;
			label.enableWordWrapping = true;
			label.extraPadding = true;
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
				CampaignStageVisualType.Special => new Color(0.28f, 0.82f, 0.92f, 0.94f),
				CampaignStageVisualType.Puzzle => new Color(0.72f, 0.55f, 1f, 0.96f),
				CampaignStageVisualType.Elite => new Color(0.97f, 0.54f, 0.12f, 0.94f),
				CampaignStageVisualType.Epic => new Color(0.92f, 0.18f, 0.18f, 0.94f),
				_ => new Color(0.96f, 0.8f, 0.28f, 0.9f)
			};
		}

		private static string GetStageTypeLabel(CampaignStageVisualType visualType)
		{
			return visualType switch
			{
				CampaignStageVisualType.Special => "SPECIAL",
				CampaignStageVisualType.Puzzle => "PUZZLE",
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
			var panel = CreateRect("StageDetailPanel", parent, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, -59f), new Vector2(748f, 122f));
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
			_stageDetailBody.color = new Color(0.97f, 0.97f, 0.97f, 0.98f);
			_stageDetailBody.alignment = TextAlignmentOptions.TopLeft;
			_stageDetailBody.enableWordWrapping = true;
			_stageDetailBody.overflowMode = TextOverflowModes.Truncate;
			_stageDetailBody.fontStyle = FontStyles.Normal;
			_stageDetailBody.lineSpacing = 0f;
			_stageDetailBody.wordWrappingRatios = 0.35f;
			_stageDetailBody.margin = new Vector4(0f, 0f, 8f, 0f);

			_stageDetailInstruction = CreateText("DetailInstruction", panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, -126f), new Vector2(536f, 28f), 14.5f, "");
			_stageDetailInstruction.color = new Color(0.98f, 0.88f, 0.54f, 0.98f);
			_stageDetailInstruction.alignment = TextAlignmentOptions.TopLeft;
			_stageDetailInstruction.enableWordWrapping = true;
			_stageDetailInstruction.overflowMode = TextOverflowModes.Ellipsis;
			_stageDetailInstruction.fontStyle = FontStyles.Bold;
			_stageDetailInstruction.lineSpacing = 1f;
			_stageDetailInstruction.margin = new Vector4(0f, 0f, 10f, 0f);

			var helper = CreateText("DetailHelper", panel, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-14f, 12f), new Vector2(210f, 14f), 9f, "Hover a node to inspect it");
			helper.alignment = TextAlignmentOptions.BottomRight;
			helper.color = new Color(0.84f, 0.84f, 0.88f, 0.62f);
		}

		private void UpdateStageDetails(CampaignQuadrantDefinition quadrant, CampaignStageDefinition stage, CampaignNodeState state)
		{
			if (_stageDetailTitle == null)
				return;

			var stageProgress = GetStageProgress(quadrant, stage.Index);
			_stageDetailTitle.text = _isHeroicMode ? $"{stage.Title}  •  HEROIC" : stage.Title;
			_stageDetailBody.text = GetStageDetailBody(quadrant, stage, stageProgress, state);
			if (_stageDetailInstruction != null)
				_stageDetailInstruction.text = GetStageInstruction(quadrant, stage, stageProgress, state);
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
				CampaignStageVisualType.Special => "Special-rule encounter with a custom opening battlefield twist.",
				CampaignStageVisualType.Puzzle => "Puzzle duel with an asymmetric opening rule to solve.",
				CampaignStageVisualType.Elite => "Elite fight with a stronger enemy opener.",
				CampaignStageVisualType.Epic => "Epic boss fight with a heavy advantage.",
				_ => "Regular fight that advances your run."
			};
		}

		private static string BuildTreasureBonusText(CampaignTreasureChoiceModel treasure)
		{
			if (treasure.BonusHeroHp > 0 && treasure.BonusCards > 0)
				return $"+{treasure.BonusHeroHp} HP  •  +{treasure.BonusCards} card";
			if (treasure.BonusHeroHp > 0)
				return $"+{treasure.BonusHeroHp} HP each battle";
			return $"+{treasure.BonusCards} card each battle";
		}

		private static string GetStageDetailBody(CampaignQuadrantDefinition quadrant, CampaignStageDefinition stage, CampaignStageProgressModel stageProgress, CampaignNodeState state)
		{
			if (state == CampaignNodeState.Locked)
				return "Locked encounter. Clear the current node first.";

			if (state == CampaignNodeState.Cleared)
				return $"Cleared with {Mathf.Max(stageProgress?.Stars ?? 0, 1)} star(s). Replay to improve.";

			if (!string.IsNullOrWhiteSpace(stageProgress?.RuleText))
				return FormatStageRuleText(stageProgress.RuleText);

			return GetStageFlavor(stage);
		}

		private static string GetStageInstruction(CampaignQuadrantDefinition quadrant, CampaignStageDefinition stage, CampaignStageProgressModel stageProgress, CampaignNodeState state)
		{
			if (state == CampaignNodeState.Cleared)
				return "Replay for a better star score and cleaner finish.";
			if (state == CampaignNodeState.Locked)
				return $"Beat the current {quadrant.DisplayName} node to unlock this.";

			return stage.VisualType switch
			{
				CampaignStageVisualType.Puzzle => "Puzzle duel. Read the special rule, then click to start.",
				CampaignStageVisualType.Special => "Special-rule encounter. Preview the opening rule before you commit.",
				CampaignStageVisualType.Elite => "Elite encounter. Stronger opener, better rewards.",
				CampaignStageVisualType.Epic => "Epic boss encounter. Major checkpoint fight.",
				_ => "Win to advance. Higher stars come from cleaner finishes."
			};
		}

		private static string FormatStageRuleText(string ruleText)
		{
			if (string.IsNullOrWhiteSpace(ruleText))
				return string.Empty;

			return ruleText
				.Replace("Enemy starts with", "Enemy: ")
				.Replace("You start with", "You: ")
				.Replace("You starts with", "You: ")
				.Replace(". You:", ".\nYou:")
				.Replace(". Heroic", ".\nHeroic")
				.Replace(". Frozen", ".\nFrozen")
				.Replace(". Overgrown", ".\nOvergrown")
				.Replace(". Dune", ".\nDune")
				.Replace(". Funeral", ".\nFuneral")
				.Replace(". Siege", ".\nSiege")
				.Replace(". Crypt", ".\nCrypt");
		}

		private static bool IsStaticHeroic(CampaignStageProgressModel stageProgress)
		{
			return stageProgress?.IsHeroic == true;
		}

		private List<CampaignStageProgressModel> GetDisplayedStageProgresses(CampaignQuadrantProgressModel quadrantProgress)
		{
			if (quadrantProgress == null)
				return null;

			return _isHeroicMode ? quadrantProgress.HeroicStages : quadrantProgress.Stages;
		}

		private static List<CampaignStageProgressModel> GetDisplayedStageProgresses(CampaignQuadrantProgressModel quadrantProgress, bool isHeroic)
		{
			if (quadrantProgress == null)
				return null;

			return isHeroic ? quadrantProgress.HeroicStages : quadrantProgress.Stages;
		}

		private string BuildCampaignResultSummary(CampaignRedirectArg result, CampaignQuadrantDefinition quadrant, CampaignQuadrantProgressModel quadrantProgress, CampaignStageProgressModel stageProgress)
		{
			var stageTitle = result.StageTitle ?? stageProgress?.Title ?? quadrant.DisplayName;
			if (!result.DidWin)
				return $"{stageTitle} was not cleared. Refit your deck, review the rule text, and retry the current encounter.";

			var stars = Mathf.Clamp(stageProgress?.Stars ?? 0, 0, 3);
			if (stars > 0)
				return $"{stageTitle} is cleared with {stars} star{(stars == 1 ? string.Empty : "s")}. Your campaign path advances from here.";

			return $"{stageTitle} is cleared. Campaign progress has been updated.";
		}

		private string BuildCampaignNextStep(CampaignRedirectArg result, CampaignQuadrantProgressModel quadrantProgress, CampaignStageProgressModel stageProgress)
		{
			if (!result.DidWin)
				return "Retry this encounter, switch decks, or return stronger after reviewing its battlefield rule.";

			if (_campaignProgress?.PendingTreasureChoices?.Count > 0)
				return "A treasure reward is waiting. Choose 1 before starting your next campaign stage.";

			if (result.Quadrant == Quadrant.Vulcan_City && quadrantProgress?.IsCompleted == true)
				return "Vulcan City is complete. Your finale progress and placeholder rewards are now recorded.";

			if (result.IsHeroic && quadrantProgress?.IsHeroicCompleted == true)
				return $"{quadrantProgress.DisplayName} Heroic is fully cleared.";

			if (!result.IsHeroic && quadrantProgress?.IsCompleted == true)
			{
				if (_campaignProgress?.IsVulcanCityUnlocked == true && _campaignProgress.CompletedCoreQuadrants >= 4)
					return $"{quadrantProgress.DisplayName} is cleared. Vulcan City is now unlocked.";
				if (quadrantProgress.IsHeroicUnlocked)
					return $"{quadrantProgress.DisplayName} is cleared. Heroic mode is now unlocked for this quadrant.";
				return $"{quadrantProgress.DisplayName} is cleared.";
			}

			var nextStage = GetDisplayedStageProgresses(quadrantProgress, result.IsHeroic)?
				.FirstOrDefault(x => x.IsUnlocked && !x.IsCompleted && x.StageIndex != result.StageIndex);
			if (nextStage != null)
				return $"{nextStage.Title} is now unlocked.";

			return stageProgress?.StarRewardText ?? "Campaign progress updated.";
		}

		private enum CampaignNodeState
		{
			Current,
			Cleared,
			Locked
		}
	}
}
