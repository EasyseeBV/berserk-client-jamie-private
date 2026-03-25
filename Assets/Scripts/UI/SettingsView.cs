using System;
using System.Collections.Generic;
using BerserkV3.Common.SceneService;
using BerserkV3.Common.TutorialSystem;
using BerserkV3.Common.Utils;
using Cysharp.Threading.Tasks;
using Events;
using RR.Core.Extensions;
using RR.Core.ResourceManagament;
using RR.Game.TutorialSystemV2.Realizations;
using RR.UI.FrameSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
	[Obsolete("Need to rework")]
	public partial class SettingsView : BaseView
	{
		private enum SettingsTab
		{
			Audio,
			Visual
		}

		private sealed class ArenaThemeButtonState
		{
			public string Value;
			public Image Frame;
			public Outline Border;
			public Image Glow;
			public TextMeshProUGUI Label;
			public TextMeshProUGUI Marker;
		}

		private readonly List<ArenaThemeButtonState> arenaThemeButtons = new();
		private RectTransform boardLayoutContainer;
		private RectTransform arenaThemeContainer;
		private RectTransform tabContainer;
		private RectTransform audioPanelRoot;
		private RectTransform visualPanelRoot;
		private RectTransform footerRoot;
		private Image audioTabBackground;
		private Image visualTabBackground;
		private Image audioTabUnderline;
		private Image visualTabUnderline;
		private Image boardLayoutToggleBackground;
		private TextMeshProUGUI audioTabLabel;
		private TextMeshProUGUI visualTabLabel;
		private TextMeshProUGUI boardLayoutToggleLabel;
		private Vector2 originalContainerSize;
		private bool settingsLayoutCached;
		private bool activeTabInitialized;
		private SettingsTab activeTab;

		protected override void Start()
		{
			base.Start();
			MusicSlider.onValueChanged.AddListener(OnMusicVolumeUpdate);
			AudioSlider.onValueChanged.AddListener(OnAudioVolumeUpdate);
			CloseBtn.Subscribe(Close);
			RestartTutorBtn.Subscribe(RestartTutorial);
			RestartTutorBtn.Subscribe(Close);
			SceneServiceAdapter.Service.OnSceneLoaded += OnSceneLoaded;
			ArenaThemeSettings.Changed += RefreshArenaThemeSelectionState;
			BoardLayoutSettings.Changed += RefreshBoardLayoutSelectionState;
			OnSceneLoaded(SceneServiceAdapter.Service.Current);
			BuildArenaThemePicker();
			BuildTabs();
			ApplyExtendedSettingsLayout();
			SetActiveTab(SettingsTab.Visual);
		}

		private void OnSceneLoaded(Scene scene)
		{
			var inGame = scene == Scene.Game;
			SetActive(RestartTutorBtn, !inGame);
		}

		private async void RestartTutorial()
		{
			await TutorialAdapter.Application.RestartAsync().AddLoadingTask();
			await TutorialAdapter.Application.InvokeAsync(TutorialTrigger.StartSession.ToString()).AddLoadingTask();
		}

		protected override void OnShown()
		{
			AudioSlider.value = DataBus.AppData.Value.AudioVolume;
			MusicSlider.value = DataBus.AppData.Value.MusicVolume;
			RefreshArenaThemeSelectionState();
			RefreshBoardLayoutSelectionState();
			ApplyExtendedSettingsLayout();
			SetActiveTab(activeTabInitialized ? activeTab : SettingsTab.Visual);
		}

		private void OnMusicVolumeUpdate(float volume)
		{
			DataBus.AppData.Value.MusicVolume = volume;
			DataBus.AppData.Repeat();
		}

		private void OnAudioVolumeUpdate(float volume)
		{
			DataBus.AppData.Value.AudioVolume = volume;
			DataBus.AppData.Repeat();
		}

		private void BuildArenaThemePicker()
		{
			if (!Container || arenaThemeContainer)
				return;

			arenaThemeContainer = new GameObject("ArenaThemeContainer", typeof(RectTransform)).GetComponent<RectTransform>();
			arenaThemeContainer.SetParent(visualPanelRoot ? visualPanelRoot : Container, false);
			arenaThemeContainer.anchorMin = new Vector2(0.5f, 0.5f);
			arenaThemeContainer.anchorMax = new Vector2(0.5f, 0.5f);
			arenaThemeContainer.pivot = new Vector2(0.5f, 0.5f);
			arenaThemeContainer.sizeDelta = new Vector2(920f, 390f);
			arenaThemeContainer.anchoredPosition = new Vector2(0f, -6f);

			CreateArenaThemePanel();
			CreateArenaThemeLabel(new Vector2(0f, 92f), new Vector2(340f, 36f));
			CreateArenaThemeHint(new Vector2(0f, 62f), new Vector2(500f, 26f));
			CreateArenaThemeButtons();
			BuildBoardLayoutToggle();
			RefreshArenaThemeSelectionState();
			RefreshBoardLayoutSelectionState();
		}

		private void ApplyExtendedSettingsLayout()
		{
			if (!Container)
				return;

			if (!settingsLayoutCached)
			{
				originalContainerSize = Container.sizeDelta;
				settingsLayoutCached = true;
			}

			Container.sizeDelta = new Vector2(originalContainerSize.x, Mathf.Max(originalContainerSize.y, 1000f));
			BuildLayoutRoots();

			if (tabContainer)
			{
				tabContainer.anchorMin = new Vector2(0.5f, 1f);
				tabContainer.anchorMax = new Vector2(0.5f, 1f);
				tabContainer.pivot = new Vector2(0.5f, 1f);
				tabContainer.sizeDelta = new Vector2(520f, 50f);
				tabContainer.anchoredPosition = new Vector2(0f, -118f);
			}

			if (arenaThemeContainer)
			{
				arenaThemeContainer.anchorMin = new Vector2(0.5f, 1f);
				arenaThemeContainer.anchorMax = new Vector2(0.5f, 1f);
				arenaThemeContainer.pivot = new Vector2(0.5f, 1f);
				arenaThemeContainer.sizeDelta = new Vector2(1000f, 390f);
				arenaThemeContainer.anchoredPosition = new Vector2(0f, -8f);
			}

			if (boardLayoutContainer)
			{
				boardLayoutContainer.anchorMin = new Vector2(0.5f, 1f);
				boardLayoutContainer.anchorMax = new Vector2(0.5f, 1f);
				boardLayoutContainer.pivot = new Vector2(0.5f, 1f);
				boardLayoutContainer.sizeDelta = new Vector2(900f, 108f);
				boardLayoutContainer.anchoredPosition = new Vector2(0f, -216f);
			}

			if (MusicContainer)
			{
				MusicContainer.anchorMin = new Vector2(0.5f, 0.5f);
				MusicContainer.anchorMax = new Vector2(0.5f, 0.5f);
				MusicContainer.pivot = new Vector2(0.5f, 0.5f);
				MusicContainer.localScale = Vector3.one;
				MusicContainer.sizeDelta = new Vector2(822.494f, 100f);
				MusicContainer.anchoredPosition = new Vector2(0f, -24f);
			}

			if (AudioContainer)
			{
				AudioContainer.anchorMin = new Vector2(0.5f, 0.5f);
				AudioContainer.anchorMax = new Vector2(0.5f, 0.5f);
				AudioContainer.pivot = new Vector2(0.5f, 0.5f);
				AudioContainer.localScale = Vector3.one;
				AudioContainer.sizeDelta = new Vector2(822.494f, 100f);
				AudioContainer.anchoredPosition = new Vector2(0f, -136f);
			}

			if (ButtonsPanel)
			{
				ButtonsPanel.anchorMin = new Vector2(0.5f, 0.5f);
				ButtonsPanel.anchorMax = new Vector2(0.5f, 0.5f);
				ButtonsPanel.pivot = new Vector2(0.5f, 0.5f);
				ButtonsPanel.localScale = Vector3.one;
				ButtonsPanel.sizeDelta = new Vector2(620f, 92f);
				ButtonsPanel.anchoredPosition = Vector2.zero;
			}

			ApplySectionLabelStyle(MusicLabel);
			ApplySectionLabelStyle(AudioLabel);

			if (tabContainer)
				tabContainer.SetAsLastSibling();
		}

		private void BuildLayoutRoots()
		{
			if (!Container)
				return;

			if (!audioPanelRoot)
			{
				audioPanelRoot = new GameObject("AudioPanelRoot", typeof(RectTransform)).GetComponent<RectTransform>();
				audioPanelRoot.SetParent(Container, false);
				audioPanelRoot.anchorMin = new Vector2(0.5f, 1f);
				audioPanelRoot.anchorMax = new Vector2(0.5f, 1f);
				audioPanelRoot.pivot = new Vector2(0.5f, 1f);
				audioPanelRoot.sizeDelta = new Vector2(1040f, 310f);
				audioPanelRoot.anchoredPosition = new Vector2(0f, -182f);
			}

			if (!visualPanelRoot)
			{
				visualPanelRoot = new GameObject("VisualPanelRoot", typeof(RectTransform)).GetComponent<RectTransform>();
				visualPanelRoot.SetParent(Container, false);
				visualPanelRoot.anchorMin = new Vector2(0.5f, 1f);
				visualPanelRoot.anchorMax = new Vector2(0.5f, 1f);
				visualPanelRoot.pivot = new Vector2(0.5f, 1f);
				visualPanelRoot.sizeDelta = new Vector2(1040f, 330f);
				visualPanelRoot.anchoredPosition = new Vector2(0f, -182f);
			}

			if (!footerRoot)
			{
				footerRoot = new GameObject("SettingsFooterRoot", typeof(RectTransform)).GetComponent<RectTransform>();
				footerRoot.SetParent(Container, false);
				footerRoot.anchorMin = new Vector2(0.5f, 0f);
				footerRoot.anchorMax = new Vector2(0.5f, 0f);
				footerRoot.pivot = new Vector2(0.5f, 0f);
				footerRoot.sizeDelta = new Vector2(620f, 92f);
				footerRoot.anchoredPosition = new Vector2(0f, 52f);
			}

			if (MusicContainer && MusicContainer.parent != audioPanelRoot)
				MusicContainer.SetParent(audioPanelRoot, false);

			if (AudioContainer && AudioContainer.parent != audioPanelRoot)
				AudioContainer.SetParent(audioPanelRoot, false);

			if (arenaThemeContainer && arenaThemeContainer.parent != visualPanelRoot)
				arenaThemeContainer.SetParent(visualPanelRoot, false);

			if (ButtonsPanel && ButtonsPanel.parent != footerRoot)
				ButtonsPanel.SetParent(footerRoot, false);

			if (audioPanelRoot)
				audioPanelRoot.localScale = Vector3.one;

			if (visualPanelRoot)
				visualPanelRoot.localScale = Vector3.one;

			if (footerRoot)
				footerRoot.localScale = Vector3.one;
		}

		private static void ApplySectionLabelStyle(TextMeshProUGUI label)
		{
			if (!label)
				return;

			label.enableAutoSizing = false;
			label.fontSize = 24f;
			label.fontStyle = FontStyles.Bold;
			label.color = new Color(0.95f, 0.85f, 0.6f, 1f);
			label.alignment = TextAlignmentOptions.Center;
		}

		private void BuildTabs()
		{
			if (!Container || tabContainer)
				return;

			tabContainer = new GameObject("SettingsTabs", typeof(RectTransform)).GetComponent<RectTransform>();
			tabContainer.SetParent(Container, false);
			tabContainer.anchorMin = new Vector2(0.5f, 0.5f);
			tabContainer.anchorMax = new Vector2(0.5f, 0.5f);
			tabContainer.pivot = new Vector2(0.5f, 0.5f);
			tabContainer.SetAsLastSibling();

			(audioTabBackground, audioTabLabel, audioTabUnderline) =
				CreateTabButton("AudioTab", new Vector2(-142f, 0f), "AUDIO", () => SetActiveTab(SettingsTab.Audio));
			(visualTabBackground, visualTabLabel, visualTabUnderline) =
				CreateTabButton("VisualTab", new Vector2(142f, 0f), "VISUAL", () => SetActiveTab(SettingsTab.Visual));
		}

		private (Image background, TextMeshProUGUI label, Image underline) CreateTabButton(string objectName, Vector2 position, string labelText, Action onClick)
		{
			var buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
			buttonObject.transform.SetParent(tabContainer, false);

			var rectTransform = buttonObject.GetComponent<RectTransform>();
			rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
			rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
			rectTransform.anchoredPosition = position;
			rectTransform.sizeDelta = new Vector2(224f, 42f);

			var background = buttonObject.GetComponent<Image>();
			background.color = new Color(0.18f, 0.12f, 0.08f, 0.92f);

			var button = buttonObject.GetComponent<Button>();
			var colors = button.colors;
			colors.normalColor = Color.white;
			colors.highlightedColor = new Color(1f, 0.95f, 0.85f, 1f);
			colors.pressedColor = new Color(0.9f, 0.85f, 0.75f, 1f);
			button.colors = colors;
			button.targetGraphic = background;
			button.onClick.AddListener(() => onClick?.Invoke());

			var label = CreateStandaloneText($"{objectName}_Label", Vector2.zero, new Vector2(220f, 32f), 20f, buttonObject.transform);
			label.alignment = TextAlignmentOptions.Center;
			label.fontStyle = FontStyles.Bold;
			label.color = new Color(0.95f, 0.85f, 0.6f, 1f);
			label.text = labelText;

			var underlineObject = new GameObject($"{objectName}_Underline", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
			underlineObject.transform.SetParent(buttonObject.transform, false);
			var underlineRect = underlineObject.GetComponent<RectTransform>();
			underlineRect.anchorMin = new Vector2(0.5f, 0f);
			underlineRect.anchorMax = new Vector2(0.5f, 0f);
			underlineRect.pivot = new Vector2(0.5f, 0f);
			underlineRect.anchoredPosition = Vector2.zero;
			underlineRect.sizeDelta = new Vector2(160f, 4f);
			var underline = underlineObject.GetComponent<Image>();
			underline.color = new Color(1f, 0.72f, 0.2f, 0.95f);

			return (background, label, underline);
		}

		private void SetActiveTab(SettingsTab tab)
		{
			activeTabInitialized = true;
			activeTab = tab;

			var isVisual = tab == SettingsTab.Visual;
			SetActive(MusicContainer, !isVisual);
			SetActive(AudioContainer, !isVisual);
			SetActive(arenaThemeContainer, isVisual);

			ApplyTabVisualState(audioTabBackground, audioTabLabel, !isVisual);
			ApplyTabVisualState(visualTabBackground, visualTabLabel, isVisual);
			if (audioTabUnderline)
				audioTabUnderline.enabled = !isVisual;
			if (visualTabUnderline)
				visualTabUnderline.enabled = isVisual;
		}

		private void ApplyTabVisualState(Image background, TextMeshProUGUI label, bool selected)
		{
			if (!background || !label)
				return;

			background.color = selected
				? new Color(0.88f, 0.66f, 0.18f, 0.95f)
				: new Color(0.18f, 0.12f, 0.08f, 0.92f);

			label.color = selected
				? new Color(0.18f, 0.1f, 0.04f, 1f)
				: new Color(0.95f, 0.85f, 0.6f, 1f);
		}

		private void CreateArenaThemeButtons()
		{
			var definitions = ArenaThemeSettings.GetDefinitions();
			const float step = 156f;
			var startX = -((definitions.Length - 1) * step * 0.5f);

			for (var i = 0; i < definitions.Length; i++)
			{
				CreateArenaThemeButton(definitions[i], new Vector2(startX + (i * step), -36f));
			}
		}

		private void CreateArenaThemePanel()
		{
			var panelObject = new GameObject(
				"ArenaThemePanel",
				typeof(RectTransform),
				typeof(CanvasRenderer),
				typeof(Image));
			panelObject.transform.SetParent(arenaThemeContainer, false);
			panelObject.transform.SetAsFirstSibling();

			var rectTransform = panelObject.GetComponent<RectTransform>();
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;

			var image = panelObject.GetComponent<Image>();
			image.color = new Color(0f, 0f, 0f, 0f);
		}

		private void CreateArenaThemeButton(ArenaThemeDefinition definition, Vector2 position)
		{
			var buttonGameObject = new GameObject(
				$"ArenaTheme_{definition.PreferenceValue}",
				typeof(RectTransform),
				typeof(CanvasRenderer),
				typeof(Image),
				typeof(Button));
			buttonGameObject.transform.SetParent(arenaThemeContainer, false);

			var rectTransform = buttonGameObject.GetComponent<RectTransform>();
			rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
			rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
			rectTransform.anchoredPosition = position;
			rectTransform.sizeDelta = new Vector2(140f, 140f);

			var frameImage = buttonGameObject.GetComponent<Image>();
			frameImage.color = new Color(0f, 0f, 0f, 0f);

			var border = buttonGameObject.AddComponent<Outline>();
			border.effectDistance = new Vector2(2f, -2f);
			border.useGraphicAlpha = false;
			border.effectColor = new Color(0.42f, 0.42f, 0.42f, 0.95f);

			var button = buttonGameObject.GetComponent<Button>();
			var colors = button.colors;
			colors.normalColor = frameImage.color;
			colors.highlightedColor = new Color(1f, 1f, 1f, 0.08f);
			colors.pressedColor = new Color(1f, 1f, 1f, 0.14f);
			colors.selectedColor = colors.normalColor;
			button.colors = colors;
			button.onClick.AddListener(() => ArenaThemeSettings.SetSelected(definition.PreferenceValue));

			var glowObject = new GameObject("Glow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
			glowObject.transform.SetParent(buttonGameObject.transform, false);
			var glowRect = glowObject.GetComponent<RectTransform>();
			glowRect.anchorMin = new Vector2(0.5f, 1f);
			glowRect.anchorMax = new Vector2(0.5f, 1f);
			glowRect.pivot = new Vector2(0.5f, 1f);
			glowRect.anchoredPosition = new Vector2(0f, -4f);
			glowRect.sizeDelta = new Vector2(130f, 80f);
			var glowImage = glowObject.GetComponent<Image>();
			glowImage.color = new Color(1f, 0.72f, 0.2f, 0.08f);

			var previewFrame = new GameObject("PreviewFrame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
			previewFrame.transform.SetParent(buttonGameObject.transform, false);
			var previewFrameRect = previewFrame.GetComponent<RectTransform>();
			previewFrameRect.anchorMin = new Vector2(0.5f, 1f);
			previewFrameRect.anchorMax = new Vector2(0.5f, 1f);
			previewFrameRect.pivot = new Vector2(0.5f, 1f);
			previewFrameRect.anchoredPosition = new Vector2(0f, -6f);
			previewFrameRect.sizeDelta = new Vector2(130f, 78f);
			var previewFrameImage = previewFrame.GetComponent<Image>();
			previewFrameImage.color = new Color(0.68f, 0.68f, 0.68f, 0.9f);

			var previewBackground = new GameObject("Preview", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
			previewBackground.transform.SetParent(buttonGameObject.transform, false);
			var previewRect = previewBackground.GetComponent<RectTransform>();
			previewRect.anchorMin = new Vector2(0.5f, 1f);
			previewRect.anchorMax = new Vector2(0.5f, 1f);
			previewRect.pivot = new Vector2(0.5f, 1f);
			previewRect.anchoredPosition = new Vector2(0f, -8f);
			previewRect.sizeDelta = new Vector2(124f, 72f);
			var previewRawImage = previewBackground.GetComponent<RawImage>();
			previewRawImage.color = Color.white;
			previewRawImage.LoadResourceAsync(definition.BackgroundResourceId).Forget();

			var boardPreview = new GameObject("BoardPreview", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
			boardPreview.transform.SetParent(buttonGameObject.transform, false);
			var boardRect = boardPreview.GetComponent<RectTransform>();
			boardRect.anchorMin = new Vector2(0.5f, 1f);
			boardRect.anchorMax = new Vector2(0.5f, 1f);
			boardRect.pivot = new Vector2(0.5f, 1f);
			boardRect.anchoredPosition = new Vector2(0f, -48f);
			boardRect.sizeDelta = new Vector2(96f, 28f);
			var boardRawImage = boardPreview.GetComponent<RawImage>();
			boardRawImage.color = Color.white;
			boardRawImage.LoadResourceAsync(definition.GameboardResourceId).Forget();

			var title = CreateText($"{definition.PreferenceValue}_Label", new Vector2(0f, -56f), new Vector2(140f, 26f), 16f, buttonGameObject.transform);
			title.alignment = TextAlignmentOptions.Bottom;
			title.fontStyle = FontStyles.Bold;
			title.color = new Color(0.98f, 0.84f, 0.38f, 1f);
			title.text = definition.DisplayName.ToUpperInvariant();

			var marker = CreateText($"{definition.PreferenceValue}_Selected", new Vector2(0f, 44f), new Vector2(100f, 18f), 12f, buttonGameObject.transform);
			marker.alignment = TextAlignmentOptions.Center;
			marker.color = new Color(1f, 0.86f, 0.38f, 1f);

			var trigger = buttonGameObject.AddComponent<EventTrigger>();
			AddHoverEvent(trigger, EventTriggerType.PointerEnter, () =>
			{
				if (button != null)
					button.targetGraphic.CrossFadeAlpha(0.1f, 0.1f, true);
			});
			AddHoverEvent(trigger, EventTriggerType.PointerExit, () =>
			{
				if (button != null)
					button.targetGraphic.CrossFadeAlpha(0f, 0.1f, true);
			});

			arenaThemeButtons.Add(new ArenaThemeButtonState
			{
				Value = definition.PreferenceValue,
				Frame = frameImage,
				Border = border,
				Glow = glowImage,
				Label = title,
				Marker = marker
			});
		}

		private void CreateArenaThemeLabel(Vector2 position, Vector2 size)
		{
			var label = CreateText("ArenaThemeLabel", position, size, 28f);
			label.alignment = TextAlignmentOptions.Center;
			label.fontStyle = FontStyles.Bold;
			label.color = new Color(0.98f, 0.82f, 0.36f, 1f);
			label.text = "Arena Skin";
		}

		private void CreateArenaThemeHint(Vector2 position, Vector2 size)
		{
			var hint = CreateText("ArenaThemeHint", position, size, 16f);
			hint.alignment = TextAlignmentOptions.Center;
			hint.color = new Color(0.84f, 0.84f, 0.84f, 0.95f);
			hint.text = "Choose the battlefield theme used in your matches";
		}

		private void BuildBoardLayoutToggle()
		{
			if (!arenaThemeContainer || boardLayoutContainer)
				return;

			boardLayoutContainer = new GameObject("BoardLayoutContainer", typeof(RectTransform)).GetComponent<RectTransform>();
			boardLayoutContainer.SetParent(arenaThemeContainer, false);
			boardLayoutContainer.anchorMin = new Vector2(0.5f, 0.5f);
			boardLayoutContainer.anchorMax = new Vector2(0.5f, 0.5f);
			boardLayoutContainer.pivot = new Vector2(0.5f, 0.5f);
			boardLayoutContainer.sizeDelta = new Vector2(900f, 108f);
			boardLayoutContainer.anchoredPosition = new Vector2(0f, -216f);

			var title = CreateText("BoardLayoutLabel", new Vector2(-308f, 10f), new Vector2(260f, 34f), 24f, boardLayoutContainer);
			title.alignment = TextAlignmentOptions.Left;
			title.fontStyle = FontStyles.Bold;
			title.color = new Color(0.95f, 0.85f, 0.6f, 1f);
			title.text = "Board Layout";

			var hint = CreateText("BoardLayoutHint", new Vector2(-258f, -20f), new Vector2(360f, 24f), 14f, boardLayoutContainer);
			hint.alignment = TextAlignmentOptions.Left;
			hint.color = new Color(0.84f, 0.84f, 0.84f, 0.9f);
			hint.text = "Switch between the classic board and the minimal full-background layout";

			var toggleObject = new GameObject(
				"BoardLayoutToggle",
				typeof(RectTransform),
				typeof(CanvasRenderer),
				typeof(Image),
				typeof(Button));
			toggleObject.transform.SetParent(boardLayoutContainer, false);

			var toggleRect = toggleObject.GetComponent<RectTransform>();
			toggleRect.anchorMin = new Vector2(1f, 0.5f);
			toggleRect.anchorMax = new Vector2(1f, 0.5f);
			toggleRect.pivot = new Vector2(1f, 0.5f);
			toggleRect.anchoredPosition = new Vector2(-20f, 0f);
			toggleRect.sizeDelta = new Vector2(230f, 58f);

			boardLayoutToggleBackground = toggleObject.GetComponent<Image>();
			boardLayoutToggleBackground.color = new Color(0.18f, 0.12f, 0.08f, 0.92f);

			var button = toggleObject.GetComponent<Button>();
			var colors = button.colors;
			colors.normalColor = Color.white;
			colors.highlightedColor = new Color(1f, 0.95f, 0.85f, 1f);
			colors.pressedColor = new Color(0.9f, 0.85f, 0.75f, 1f);
			button.colors = colors;
			button.targetGraphic = boardLayoutToggleBackground;
			button.onClick.AddListener(() => BoardLayoutSettings.SetCompact(!BoardLayoutSettings.IsMinimal()));

			boardLayoutToggleLabel = CreateStandaloneText(
				"BoardLayoutToggleLabel",
				Vector2.zero,
				new Vector2(210f, 32f),
				20f,
				toggleObject.transform);
			boardLayoutToggleLabel.alignment = TextAlignmentOptions.Center;
			boardLayoutToggleLabel.fontStyle = FontStyles.Bold;
		}

		private TextMeshProUGUI CreateText(string objectName, Vector2 position, Vector2 size, float fontSize, Transform parentOverride = null)
		{
			var textGameObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
			textGameObject.transform.SetParent(parentOverride ? parentOverride : arenaThemeContainer, false);

			var rectTransform = textGameObject.GetComponent<RectTransform>();
			rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
			rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
			rectTransform.anchoredPosition = position;
			rectTransform.sizeDelta = size;

			var text = textGameObject.GetComponent<TextMeshProUGUI>();
			text.fontSize = fontSize;
			text.enableWordWrapping = false;
			text.raycastTarget = false;
			text.text = string.Empty;
			return text;
		}

		private TextMeshProUGUI CreateStandaloneText(string objectName, Vector2 position, Vector2 size, float fontSize, Transform parent)
		{
			var textGameObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
			textGameObject.transform.SetParent(parent, false);

			var rectTransform = textGameObject.GetComponent<RectTransform>();
			rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
			rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
			rectTransform.anchoredPosition = position;
			rectTransform.sizeDelta = size;

			var text = textGameObject.GetComponent<TextMeshProUGUI>();
			text.fontSize = fontSize;
			text.enableWordWrapping = false;
			text.raycastTarget = false;
			return text;
		}

		private void RefreshArenaThemeSelectionState()
		{
			var selectedValue = ArenaThemeSettings.GetSelectedValue();
			foreach (var button in arenaThemeButtons)
			{
				var isSelected = button.Value == selectedValue;
				button.Frame.color = isSelected
					? new Color(1f, 0.78f, 0.24f, 0.16f)
					: new Color(1f, 1f, 1f, 0f);
				if (button.Border)
				{
					button.Border.effectDistance = isSelected ? new Vector2(3f, -3f) : new Vector2(2f, -2f);
					button.Border.effectColor = isSelected
						? new Color(1f, 0.72f, 0.2f, 1f)
						: new Color(0.48f, 0.48f, 0.48f, 0.95f);
				}
				if (button.Glow)
					button.Glow.enabled = isSelected;
				if (button.Label)
				{
					button.Label.color = isSelected
						? new Color(1f, 0.86f, 0.38f, 1f)
						: new Color(0.9f, 0.82f, 0.54f, 1f);
				}
				button.Marker.text = isSelected ? "ACTIVE" : string.Empty;
			}
		}

		private void RefreshBoardLayoutSelectionState()
		{
			if (!boardLayoutToggleBackground || !boardLayoutToggleLabel)
				return;

			var isMinimal = BoardLayoutSettings.IsMinimal();
			boardLayoutToggleBackground.color = isMinimal
				? new Color(0.88f, 0.66f, 0.18f, 0.95f)
				: new Color(0.18f, 0.12f, 0.08f, 0.92f);
			boardLayoutToggleLabel.color = isMinimal
				? new Color(0.18f, 0.1f, 0.04f, 1f)
				: new Color(0.95f, 0.85f, 0.6f, 1f);
			boardLayoutToggleLabel.text = isMinimal ? "MINIMAL" : "CLASSIC";
		}

		private static void AddHoverEvent(EventTrigger trigger, EventTriggerType type, Action action)
		{
			if (!trigger || action == null)
				return;

			var entry = new EventTrigger.Entry { eventID = type };
			entry.callback.AddListener(_ => action());
			trigger.triggers.Add(entry);
		}

		private void OnDestroy()
		{
			ArenaThemeSettings.Changed -= RefreshArenaThemeSelectionState;
			BoardLayoutSettings.Changed -= RefreshBoardLayoutSelectionState;
			SceneServiceAdapter.Service.OnSceneLoaded -= OnSceneLoaded;
			MusicSlider.onValueChanged.RemoveAllListeners();
			AudioSlider.onValueChanged.RemoveAllListeners();
			CloseBtn.onClick.RemoveAllListeners();
			RestartTutorBtn.onClick.RemoveAllListeners();
		}
	}
}
