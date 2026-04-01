using System;
using System.Collections.Generic;
using BerserkV3.Common.SceneService;
using Cysharp.Threading.Tasks;
using Events;
using RR.Core.Extensions;
using RR.Core.ResourceManagament;
using RR.UI.FrameSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
	/// <summary>
	/// Clean Settings view with two tabs: VISUAL (arena skin + compact board) and AUDIO (music + sfx sliders).
	/// Play Tutorial Again is deliberately hidden.
	/// </summary>
	[Obsolete("Need to rework")]
	public partial class SettingsView : BaseView
	{
		// ─── Tab enum ───────────────────────────────────────────────
		private enum SettingsTab { Visual, Audio }

		// ─── Arena button state ─────────────────────────────────────
		private sealed class ThemeBtn
		{
			public string Key;
			public Image Frame;
			public Outline Border;
			public Image Glow;
			public TextMeshProUGUI Label;
			public TextMeshProUGUI Marker;
			public Image MarkerHeader;
		}

		[Header("Button Prefab")]
		public Button ButtonPrefab;

		// ─── Runtime state ──────────────────────────────────────────
		private readonly List<ThemeBtn> _themeBtns = new();
		private RectTransform _tabBar;
		private RectTransform _visualRoot;
		private RectTransform _audioRoot;
		private RectTransform _footerRoot;
		private RectTransform _arenaBox;
		private RectTransform _layoutBox;
		private Image _visualTabBg, _audioTabBg;
		private Image _visualLine, _audioLine;
		private TextMeshProUGUI _visualTabTxt, _audioTabTxt;
		private Image _layoutToggleBg;
		private TextMeshProUGUI _layoutToggleTxt;
		private SettingsTab _tab;
		private bool _built;

		// ═════════════════════════════════════════════════════════════
		//  Lifecycle
		// ═════════════════════════════════════════════════════════════

		protected override void Start()
		{
			base.Start();

			MusicSlider.onValueChanged.AddListener(v => { DataBus.AppData.Value.MusicVolume = v; DataBus.AppData.Repeat(); });
			AudioSlider.onValueChanged.AddListener(v => { DataBus.AppData.Value.AudioVolume = v; DataBus.AppData.Repeat(); });
			CloseBtn.Subscribe(Close);

			// Play Tutorial Again remains hidden in this trimmed menu.
			SetActive(RestartTutorBtn, false);

			ArenaThemeSettings.Changed += RefreshThemeSelection;
			BoardLayoutSettings.Changed += RefreshLayoutToggle;

			BuildUI();
			SwitchTab(SettingsTab.Visual);
		}

		protected override void OnShown()
		{
			MusicSlider.value = DataBus.AppData.Value.MusicVolume;
			AudioSlider.value = DataBus.AppData.Value.AudioVolume;
			RefreshThemeSelection();
			RefreshLayoutToggle();
			SwitchTab(_tab);
		}

		private void OnDestroy()
		{
			ArenaThemeSettings.Changed -= RefreshThemeSelection;
			BoardLayoutSettings.Changed -= RefreshLayoutToggle;
			SceneServiceAdapter.Service.OnSceneLoaded -= OnSceneLoaded;
			MusicSlider.onValueChanged.RemoveAllListeners();
			AudioSlider.onValueChanged.RemoveAllListeners();
			CloseBtn.onClick.RemoveAllListeners();
		}

		private void OnSceneLoaded(Scene scene) { /* intentionally empty */ }

		// ═════════════════════════════════════════════════════════════
		//  Master build — called once
		// ═════════════════════════════════════════════════════════════

		private void BuildUI()
		{
			if (_built || !Container) return;
			_built = true;

			SceneServiceAdapter.Service.OnSceneLoaded += OnSceneLoaded;

			// Stretch container a bit so everything fits
			Container.sizeDelta = new Vector2(Container.sizeDelta.x, Mathf.Max(Container.sizeDelta.y, 1000f));

			// --- Tab bar (just below the SETTINGS title) ---
			_tabBar = MakeTopRect("TabBar", Container, -208f, new Vector2(520f, 50f));

			(_visualTabBg, _visualTabTxt) =
				MakeTab("VisualTab", _tabBar, new Vector2(-142f, 0f), "VISUAL", () => SwitchTab(SettingsTab.Visual));
			(_audioTabBg, _audioTabTxt) =
				MakeTab("AudioTab", _tabBar, new Vector2(142f, 0f), "AUDIO", () => SwitchTab(SettingsTab.Audio));

			// --- Visual panel (arena picker + board layout) ---
			_visualRoot = MakeTopRect("VisualRoot", Container, -282f, new Vector2(1040f, 360f));
			BuildArenaPicker();
			BuildLayoutToggle();

			// --- Audio panel (reparent the prefab sliders) ---
			_audioRoot = MakeTopRect("AudioRoot", Container, -282f, new Vector2(1040f, 310f));
			if (MusicContainer)
			{
				MusicContainer.SetParent(_audioRoot, false);
				ConfigureAudioSection(MusicContainer, new Vector2(822.494f, 100f), new Vector2(0f, -24f));
			}
			if (AudioContainer)
			{
				AudioContainer.SetParent(_audioRoot, false);
				ConfigureAudioSection(AudioContainer, new Vector2(822.494f, 100f), new Vector2(0f, -136f));
			}
			ApplySectionLabelStyle(MusicLabel);
			ApplySectionLabelStyle(AudioLabel);

			// --- Close button stays in footer ---
			_footerRoot = MakeBottomRect("FooterRoot", Container, 52f, new Vector2(620f, 92f));
			if (ButtonsPanel)
			{
				ButtonsPanel.SetParent(_footerRoot, false);
				ButtonsPanel.anchorMin = ButtonsPanel.anchorMax = ButtonsPanel.pivot = new Vector2(0.5f, 0.5f);
				ButtonsPanel.localScale = Vector3.one;
				ButtonsPanel.sizeDelta = new Vector2(620f, 92f);
				ButtonsPanel.anchoredPosition = Vector2.zero;
				if (CloseBtn)
				{
					var cr = CloseBtn.GetComponent<RectTransform>();
					if (cr) { cr.sizeDelta = new Vector2(420f, cr.sizeDelta.y); cr.anchoredPosition = Vector2.zero; }
				}
			}

			_tabBar.SetAsLastSibling();
		}

		// ═════════════════════════════════════════════════════════════
		//  Tab switching
		// ═════════════════════════════════════════════════════════════

		private void SwitchTab(SettingsTab tab)
		{
			_tab = tab;
			var vis = tab == SettingsTab.Visual;

			if (_visualRoot)
				SetActive(_visualRoot, vis);
			if (_audioRoot)
				SetActive(_audioRoot, !vis);

			StyleTab(_visualTabBg, vis);
			StyleTab(_audioTabBg, !vis);
		}

		private static void StyleTab(Image bg, bool on)
		{
			if (!bg) return;
			bg.color  = on ? new Color(0.04705882f, 0.7372549f, 0.6235294f, 1f) : new Color(0.04705882f, 0.7372549f, 0.6235294f, 0f);
		}

		// ═════════════════════════════════════════════════════════════
		//  Arena theme picker
		// ═════════════════════════════════════════════════════════════

		private void BuildArenaPicker()
		{
			_arenaBox = MakeRect("ArenaBox", _visualRoot, new Vector2(0f, 60f), new Vector2(980f, 280f));

			// Background panel (transparent — just a layout container)
			var panel = MakeImage("ArenaPanel", _arenaBox, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0f));
			var pr = panel.GetComponent<RectTransform>();
			pr.anchorMin = Vector2.zero; pr.anchorMax = Vector2.one;
			pr.offsetMin = Vector2.zero; pr.offsetMax = Vector2.zero;
			pr.SetAsFirstSibling();

			// Title
			var title = MakeText("ArenaTitle", _arenaBox, new Vector2(0f, 108f), new Vector2(420f, 38f), 35f);
			title.text = "ARENA SKIN";
			ApplySectionLabelStyle(title);

			// Subtitle
			var hint = MakeText("ArenaHint", _arenaBox, new Vector2(0f, 74f), new Vector2(600f, 28f), 25f);
			hint.text = "Choose the battlefield theme used in your matches";
			hint.color = new Color(0.9f, 0.9f, 0.9f, 0.98f);
			hint.alignment = TextAlignmentOptions.Center;

			// Theme buttons
			var defs = ArenaThemeSettings.GetDefinitions();
			const float stepX = 300f;
			const float stepY = 180f;
			const int columns = 3;

			// center offset
			float startX = -stepX;
			float startY = -65;

			for (int i = 0; i < defs.Length; i++)
			{
				int col = i % columns;
				int row = i / columns;

				var pos = new Vector2(
					startX + col * stepX,
					startY - row * stepY
				);

				MakeThemeButton(defs[i], pos);
			}

			RefreshThemeSelection();
		}

		private void MakeThemeButton(ArenaThemeDefinition def, Vector2 pos)
		{
			// Root button
			var go = new GameObject($"Theme_{def.PreferenceValue}",
				typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
			go.transform.SetParent(_arenaBox, false);

			float mainSize = 200;

			var rt = go.GetComponent<RectTransform>();
			rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
			rt.anchoredPosition = pos;
			rt.sizeDelta = new Vector2(mainSize, mainSize);

			var frame = go.GetComponent<Image>();
			frame.color = new Color(0f, 0f, 0f, 0f);

			var border = go.AddComponent<Outline>();
			border.effectDistance = new Vector2(2f, -2f);
			border.useGraphicAlpha = false;
			border.effectColor = new Color(0.42f, 0.42f, 0.42f, 0.95f);

			var btn = go.GetComponent<Button>();
			var c = btn.colors;
			c.normalColor = frame.color;
			c.highlightedColor = new Color(1f, 1f, 1f, 0.08f);
			c.pressedColor = new Color(1f, 1f, 1f, 0.14f);
			c.selectedColor = c.normalColor;
			btn.colors = c;
			btn.onClick.AddListener(() => ArenaThemeSettings.SetSelected(def.PreferenceValue));

			float glowX = mainSize;
			float glowY = 130;
			float diffX = 10;
			float diffY = 10;

			// Glow
			var glowImg = MakeImage("Glow", go.transform, new Vector2(0f, 0f), new Vector2(glowX, glowY),
				new Color(1f, 0.72f, 0.2f, 0.08f));
			var glowRt = glowImg.GetComponent<RectTransform>();
			glowRt.anchorMin = glowRt.anchorMax = new Vector2(0.5f, 1f);
			glowRt.pivot = new Vector2(0.5f, 1f);

			// Preview frame
			MakePreview("PreviewFrame", go.transform, new Vector2(0f, -5f), new Vector2(glowX - diffX, glowY-diffY),
				new Color(0.68f, 0.68f, 0.68f, 0.9f), true);

			// Background preview (RawImage)
			var bgPreview = MakeRawImage("BgPreview", go.transform, new Vector2(0f, -10f), new Vector2(glowX - (diffX*2), glowY - (diffY *2)));
			bgPreview.LoadResourceAsync(def.BackgroundResourceId).Forget();

			// Board preview (RawImage, smaller, overlaid)
			var boardPreview = MakeRawImage("BoardPreview", bgPreview.transform, new Vector2(0f, 0f), new Vector2(145f, 42f));
			boardPreview.rectTransform.pivot = new(0.5f, 0f);
			boardPreview.rectTransform.anchorMin = new(0.5f, 0f);
			boardPreview.rectTransform.anchorMax = new(0.5f, 0f);

			boardPreview.LoadResourceAsync(def.GameboardResourceId).Forget();

			// Theme name
			var label = MakeText($"{def.PreferenceValue}_Name", go.transform, new Vector2(0f, -54f),
				new Vector2(138f, 26f), 25f);
			label.text = def.DisplayName.ToUpperInvariant();
			label.fontStyle = FontStyles.Bold;
			label.color = new Color(0.98f, 0.84f, 0.38f, 1f);
			label.alignment = TextAlignmentOptions.Bottom;

			// "ACTIVE" marker
			var markerHeader = MakeImage("ActiveBG", go.transform, new Vector2(0f, 55f), new Vector2(0, 30f), new Color(0f,0f,0f,0.7f));
			markerHeader.rectTransform.anchorMin = new Vector2(0, 0.5f);
			markerHeader.rectTransform.anchorMax = new Vector2(1, 0.5f);
			markerHeader.rectTransform.offsetMin = new Vector2(10f, markerHeader.rectTransform.offsetMin.y);
			markerHeader.rectTransform.offsetMax = new Vector2(-10f, markerHeader.rectTransform.offsetMax.y);
			markerHeader.gameObject.SetActive(false);

			var marker = MakeText($"{def.PreferenceValue}_Active", markerHeader.transform, new Vector2(0f, 0f),
				new Vector2(100f, 18f), 20f);
			marker.text = "ACTIVE";
			marker.fontStyle = FontStyles.Bold;
			marker.alignment = TextAlignmentOptions.Center;
			marker.color = new Color(1f, 0.86f, 0.38f, 1f);

			// Hover
			var trigger = go.AddComponent<EventTrigger>();
			AddPointerEvent(trigger, EventTriggerType.PointerEnter, () => btn.targetGraphic?.CrossFadeAlpha(0.1f, 0.1f, true));
			AddPointerEvent(trigger, EventTriggerType.PointerExit,  () => btn.targetGraphic?.CrossFadeAlpha(0f, 0.1f, true));

			_themeBtns.Add(new ThemeBtn
			{
				Key = def.PreferenceValue, Frame = frame, Border = border,
				Glow = glowImg, Label = label, Marker = marker, MarkerHeader = markerHeader,
			});
		}

		private void RefreshThemeSelection()
		{
			var sel = ArenaThemeSettings.GetSelectedValue();
			foreach (var t in _themeBtns)
			{
				var on = t.Key == sel;
				t.Frame.color = on ? new Color(1f, 0.78f, 0.24f, 0.16f) : Color.clear;
				if (t.Border)
				{
					t.Border.effectDistance = on ? new Vector2(3f, -3f) : new Vector2(2f, -2f);
					t.Border.effectColor = on
						? new Color(1f, 0.72f, 0.2f, 1f)
						: new Color(0.48f, 0.48f, 0.48f, 0.95f);
				}
				if (t.Glow) t.Glow.enabled = on;
				if (t.Label) t.Label.color = on
					? new Color(1f, 0.86f, 0.38f, 1f)
					: new Color(0.9f, 0.82f, 0.54f, 1f);
				if (t.MarkerHeader) t.MarkerHeader.gameObject.SetActive(on);
			}
		}

		// ═════════════════════════════════════════════════════════════
		//  Board layout toggle
		// ═════════════════════════════════════════════════════════════

		private void BuildLayoutToggle()
		{
			_layoutBox = MakeRect("LayoutBox", _visualRoot, new Vector2(0f, -320f), new Vector2(980f, 80f));

			// Label
			var lbl = MakeText("LayoutLabel", _layoutBox, new Vector2(-240f, 8f), new Vector2(360f, 30f), 24f);
			lbl.text = "MINIMAL MODE";
			ApplySectionLabelStyle(lbl);
			lbl.alignment = TextAlignmentOptions.MidlineLeft;

			// Hint
			var hint = MakeText("LayoutHint", _layoutBox, new Vector2(-92f, -25f), new Vector2(620f, 26f), 20f);
			hint.text = "Strips away board chrome for a clean, full-screen arena";
			hint.color = new Color(0.9f, 0.9f, 0.9f, 0.98f);
			hint.alignment = TextAlignmentOptions.MidlineLeft;

			// Toggle button
			var toggleGo = new GameObject("LayoutToggle",
				typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
			toggleGo.transform.SetParent(_layoutBox, false);

			var trt = toggleGo.GetComponent<RectTransform>();
			trt.anchorMin = trt.anchorMax = new Vector2(1f, 0.5f);
			trt.pivot = new Vector2(1f, 0.5f);
			trt.anchoredPosition = new Vector2(-26f, 0f);
			trt.sizeDelta = new Vector2(160f, 48f);

			_layoutToggleBg = toggleGo.GetComponent<Image>();
			var tbtn = toggleGo.GetComponent<Button>();
			var tc = tbtn.colors;
			tc.normalColor = Color.white;
			tc.highlightedColor = new Color(1f, 0.95f, 0.85f, 1f);
			tc.pressedColor = new Color(0.9f, 0.85f, 0.75f, 1f);
			tbtn.colors = tc;
			tbtn.targetGraphic = _layoutToggleBg;
			tbtn.onClick.AddListener(() => BoardLayoutSettings.SetCompact(!BoardLayoutSettings.IsMinimal()));

			_layoutToggleTxt = MakeText("ToggleLabel", toggleGo.transform, Vector2.zero, new Vector2(140f, 34f), 18f);
			_layoutToggleTxt.alignment = TextAlignmentOptions.Center;
			_layoutToggleTxt.fontStyle = FontStyles.Bold;

			RefreshLayoutToggle();
		}

		private void RefreshLayoutToggle()
		{
			if (!_layoutToggleBg || !_layoutToggleTxt) return;
			var isMinimal = BoardLayoutSettings.IsMinimal();
			_layoutToggleBg.color   = isMinimal ? new Color(0.88f, 0.66f, 0.18f, 1f) : new Color(0.18f, 0.12f, 0.08f, 0.96f);
			_layoutToggleTxt.color  = isMinimal ? new Color(0.18f, 0.10f, 0.04f, 1f) : new Color(0.98f, 0.84f, 0.38f, 1f);
			_layoutToggleTxt.text   = isMinimal ? "ON" : "OFF";
		}

		// ═════════════════════════════════════════════════════════════
		//  Factory helpers — tiny, reusable, zero duplication
		// ═════════════════════════════════════════════════════════════

		private static void ConfigureAudioSection(RectTransform section, Vector2 size, Vector2 pos)
		{
			section.anchorMin = section.anchorMax = section.pivot = new Vector2(0.5f, 0.5f);
			section.localScale = Vector3.one;
			section.sizeDelta = size;
			section.anchoredPosition = pos;
		}

		private static void ApplySectionLabelStyle(TextMeshProUGUI label)
		{
			if (!label) return;
			label.enableAutoSizing = false;
			label.fontSize = 35f;
			label.fontStyle = FontStyles.Bold;
			label.color = new Color(0.95f, 0.85f, 0.6f, 1f);
			label.alignment = TextAlignmentOptions.Center;
		}

		private static RectTransform MakeRect(string name, Transform parent, Vector2 pos, Vector2 size)
		{
			var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
			rt.SetParent(parent, false);
			rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
			rt.anchoredPosition = pos;
			rt.sizeDelta = size;
			return rt;
		}

		private static RectTransform MakeTopRect(string name, Transform parent, float topOffset, Vector2 size)
		{
			var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
			rt.SetParent(parent, false);
			rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
			rt.pivot = new Vector2(0.5f, 1f);
			rt.anchoredPosition = new Vector2(0f, topOffset);
			rt.sizeDelta = size;
			return rt;
		}

		private static RectTransform MakeBottomRect(string name, Transform parent, float bottomOffset, Vector2 size)
		{
			var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
			rt.SetParent(parent, false);
			rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
			rt.pivot = new Vector2(0.5f, 0f);
			rt.anchoredPosition = new Vector2(0f, bottomOffset);
			rt.sizeDelta = size;
			return rt;
		}

		private static Image MakeImage(string name, Transform parent, Vector2 pos, Vector2 size, Color color)
		{
			var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
			go.transform.SetParent(parent, false);
			var rt = go.GetComponent<RectTransform>();
			rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
			rt.anchoredPosition = pos;
			rt.sizeDelta = size;
			var img = go.GetComponent<Image>();
			img.color = color;
			return img;
		}

		private static void MakePreview(string name, Transform parent, Vector2 pos, Vector2 size, Color color, bool topAnchored)
		{
			var img = MakeImage(name, parent, pos, size, color);
			if (topAnchored)
			{
				var rt = img.GetComponent<RectTransform>();
				rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
				rt.pivot = new Vector2(0.5f, 1f);
			}
		}

		private static RawImage MakeRawImage(string name, Transform parent, Vector2 pos, Vector2 size)
		{
			var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
			go.transform.SetParent(parent, false);
			var rt = go.GetComponent<RectTransform>();
			rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
			rt.pivot = new Vector2(0.5f, 1f);
			rt.anchoredPosition = pos;
			rt.sizeDelta = size;
			var ri = go.GetComponent<RawImage>();
			ri.color = Color.white;
			return ri;
		}

		private static TextMeshProUGUI MakeText(string name, Transform parent, Vector2 pos, Vector2 size, float fontSize)
		{
			var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
			go.transform.SetParent(parent, false);
			var rt = go.GetComponent<RectTransform>();
			rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
			rt.anchoredPosition = pos;
			rt.sizeDelta = size;
			var t = go.GetComponent<TextMeshProUGUI>();
			t.fontSize = fontSize;
			t.enableWordWrapping = false;
			t.raycastTarget = false;
			return t;
		}

		private (Image bg, TextMeshProUGUI txt) MakeTab(
			string name, Transform parent, Vector2 pos, string label, Action onClick)
		{
			var btn = Instantiate(ButtonPrefab);
			btn.transform.SetParent(parent, false);
			btn.gameObject.name = name;


			var rt = btn.GetComponent<RectTransform>();
			rt.sizeDelta = new Vector2(170f, 50f);
			rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
			rt.anchoredPosition = pos;

			btn.onClick.AddListener(() => onClick?.Invoke());

			var btnView = btn.GetComponent<ButtonView>();
			var bg = btnView.CustomView;

			var txt = btn.gameObject.GetComponentInChildren<TextMeshProUGUI>();
			txt.text = label;
			return (bg, txt);
		}

		private static void AddPointerEvent(EventTrigger trigger, EventTriggerType type, Action action)
		{
			var entry = new EventTrigger.Entry { eventID = type };
			entry.callback.AddListener(_ => action());
			trigger.triggers.Add(entry);
		}
	}
}
