using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using BerserkV3.GameCore.SplineSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameCoreView = BerserkV3.GameCore.UI.GameView;
using GameCoreBackgroundView = BerserkV3.GameCore.UI.BackgroundView;

namespace UI
{
	public class GameTableViewMinimal : MonoBehaviour
	{
		private sealed class RectState
		{
			public Vector2 AnchorMin;
			public Vector2 AnchorMax;
			public Vector2 Pivot;
			public Vector2 SizeDelta;
			public Vector2 AnchoredPosition;
			public Vector2 OffsetMin;
			public Vector2 OffsetMax;
			public Vector3 LocalScale;
			public Quaternion LocalRotation;
		}

		private sealed class GraphicState
		{
			public bool Enabled;
			public bool Active;
		}

		private sealed class SplineState
		{
			public float ArcSize;
			public Vector2 ArcScale;
		}

			private static GameTableViewMinimal instance;

		private readonly Dictionary<RectTransform, RectState> rectStates = new();
		private readonly Dictionary<Graphic, GraphicState> graphicStates = new();
		private readonly Dictionary<HandSpline, SplineState> splineStates = new();
		private static readonly FieldInfo ArcSizeField = typeof(HandSpline).GetField("arcSize", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly FieldInfo ArcScaleField = typeof(HandSpline).GetField("arcScale", BindingFlags.Instance | BindingFlags.NonPublic);
		private bool cached;
		private bool applied;
		private List<Transform> cachedBgRoots;
		private GameObject dividerLine;
		private GameObject minimalSurface;

		public static void EnsureInitialized()
		{
			if (instance || !GameTableView.Instance)
				return;

			instance = GameTableView.Instance.GetComponent<GameTableViewMinimal>();
			if (!instance)
				instance = GameTableView.Instance.gameObject.AddComponent<GameTableViewMinimal>();
		}

		private void Awake()
		{
			BoardLayoutSettings.Changed += ApplyCurrentLayout;
			ArenaThemeSettings.Changed += ApplyCurrentLayout;
		}

		private void OnEnable()
		{
			ApplyCurrentLayout();
		}

		private void LateUpdate()
		{
			// Only re-apply if minimal is active and we haven't applied yet
			// This avoids per-frame FindObjectsOfType spam
			if (BoardLayoutSettings.IsMinimal() && !applied)
				ApplyCurrentLayout();
			else if (!BoardLayoutSettings.IsMinimal() && applied)
				ApplyCurrentLayout();
		}

		private void ApplyCurrentLayout()
		{
			var isMinimal = BoardLayoutSettings.IsMinimal();

			// Lazy-find background roots (only once)
			if (cachedBgRoots == null || cachedBgRoots.Count == 0)
				cachedBgRoots = FindAllBackgroundRoots();

			if (!cached && cachedBgRoots.Count > 0)
				CacheOriginalValues();

			Debug.Log($"[Minimal] isMinimal={isMinimal} bgRoots={cachedBgRoots.Count} cached={cached}");

			if (isMinimal)
			{
				ApplyMinimalLayout();
				applied = true;
			}
			else
			{
				RestoreClassicLayout();
				applied = false;
			}
		}

		/// <summary>
		/// Finds ALL background roots in the scene — tries GameCore, UI, and brute-force FindObjectOfType.
		/// Returns every BackgroundView transform found so we can hide decorative elements on all of them.
		/// </summary>
		private List<Transform> FindAllBackgroundRoots()
		{
			var roots = new List<Transform>();

			var gcBg = GameCoreBackgroundView.Instance;
			if (gcBg) roots.Add(gcBg.transform);

			var uiBg = BackgroundView.Instance;
			if (uiBg && (!gcBg || uiBg.transform != gcBg.transform))
				roots.Add(uiBg.transform);

			// Brute force: find ANY BackgroundView-like object by name
			foreach (var go in FindObjectsOfType<RectTransform>(true))
			{
				if (go && (go.name.Contains("BackgrounView") || go.name.Contains("BackgroundView")))
				{
					if (!roots.Contains(go.transform))
						roots.Add(go.transform);
				}
			}

			return roots;
		}

		private void CacheOriginalValues()
		{
			cached = true;

			if (cachedBgRoots != null)
			{
				foreach (var bgRoot in cachedBgRoots)
				{
					if (!bgRoot) continue;
					CacheRect(bgRoot as RectTransform);
					foreach (var graphic in bgRoot.GetComponentsInChildren<Graphic>(true))
					{
						if (graphic) CacheGraphic(graphic);
					}
					foreach (var rt in bgRoot.GetComponentsInChildren<RectTransform>(true))
					{
						if (rt) CacheRect(rt);
					}
				}
			}

			var gameView = GameCoreView.Instance;
			if (gameView)
			{
				CacheRect(gameView.TableContainer);
				CacheRect(gameView.SelfHandContainer as RectTransform);
				CacheRect(gameView.OpponentHandContainer as RectTransform);
				CacheRect(gameView.SelfHeroContainer);
				CacheRect(gameView.OpponentHeroContainer);
			}

			foreach (var actorView in GetActorViews())
			{
				CacheRect(actorView.transform as RectTransform);
				CacheRect(actorView.GetComponentInChildren<PlayerManaPanel>(true)?.transform as RectTransform);
				CacheRect(actorView.GetComponentInChildren<OpponentManaPanel>(true)?.transform as RectTransform);
			}

			var roundButton = RoundButtonView.Instance;
			if (roundButton)
				CacheRect(FindRect(roundButton.transform, "ButtonPanel"));

			var deckCounter = FindObjectOfType<DeckCounterView>(true);
			if (deckCounter)
				CacheRect(deckCounter.transform as RectTransform);

			foreach (var graveyardCounter in FindObjectsOfType<GraveyardCounterView>(true))
				CacheRect(graveyardCounter.transform as RectTransform);

			foreach (var spline in FindObjectsOfType<HandSpline>(true))
				CacheSpline(spline);
		}

		private void ApplyMinimalLayout()
		{
			ApplyMinimalBackground();
			ApplyMinimalPlayField();
			ApplyMinimalActors();
			ApplyMinimalHud();
			ApplyMinimalHands();
			EnsureDividerLine();
		}

		private void ApplyMinimalBackground()
		{
			if (cachedBgRoots == null || cachedBgRoots.Count == 0)
				return;

			foreach (var bgRoot in cachedBgRoots)
			{
				if (!bgRoot) continue;

				StretchRect(bgRoot as RectTransform);

				Graphic[] children;
				try { children = bgRoot.GetComponentsInChildren<Graphic>(true); }
				catch { continue; }

				foreach (var graphic in children)
				{
					if (!graphic || !graphic.gameObject) continue;

					var gName = graphic.gameObject.name;

					// KEEP the full-screen background image
					if (gName == "BackgroundImage")
					{
						StretchRect(graphic.transform as RectTransform);
						continue;
					}

					// KEEP gameplay UI (counters, active zones)
					if (gName.Contains("Counter") || gName.Contains("ActiveZone"))
						continue;

					// HIDE decorative chrome: corners, table frame, plane, borders
					if (gName.Contains("Corner") || gName.Contains("Table") || gName.Contains("Plane") ||
					    gName.Contains("Border") || gName.Contains("Base") || gName.Contains("base"))
					{
						CacheGraphic(graphic);
						graphic.enabled = false;
						graphic.gameObject.SetActive(false);
					}
				}
			}
		}

		private void ApplyMinimalPlayField()
		{
			var gameView = GameCoreView.Instance;
			if (!gameView || !gameView.TableContainer)
				return;

			var table = gameView.TableContainer;
			// Narrower than before — keep cards centered, not edge-to-edge
			table.anchorMin = new Vector2(0.15f, 0.18f);
			table.anchorMax = new Vector2(0.85f, 0.78f);
			table.offsetMin = Vector2.zero;
			table.offsetMax = Vector2.zero;
			table.anchoredPosition = Vector2.zero;
			table.localScale = Vector3.one;
			EnsureMinimalSurface(table);

			// Scale down heroes so they don't obscure board cards
			if (gameView.SelfHeroContainer)
				gameView.SelfHeroContainer.localScale = Vector3.one * 0.65f;

			if (gameView.OpponentHeroContainer)
				gameView.OpponentHeroContainer.localScale = Vector3.one * 0.65f;

			// Card play area renders LAST = on top of heroes
			table.SetAsLastSibling();
		}

		private void ApplyMinimalActors()
		{
			foreach (var actorView in GetActorViews())
			{
				var rect = actorView.transform as RectTransform;
				if (!rect)
					continue;

				var isOpponent = actorView.name.Contains("Opponent");
				rect.anchorMin = isOpponent ? new Vector2(0.5f, 1f) : new Vector2(0f, 0f);
				rect.anchorMax = rect.anchorMin;
				rect.pivot = isOpponent ? new Vector2(0.5f, 1f) : new Vector2(0f, 0f);
				rect.anchoredPosition = isOpponent ? new Vector2(0f, -18f) : new Vector2(32f, 22f);
				rect.localScale = isOpponent ? Vector3.one * 0.82f : Vector3.one * 0.92f;

				var playerMana = actorView.GetComponentInChildren<PlayerManaPanel>(true);
				if (playerMana && playerMana.transform is RectTransform playerManaRect)
				{
					playerManaRect.anchorMin = new Vector2(0.5f, 0.5f);
					playerManaRect.anchorMax = new Vector2(0.5f, 0.5f);
					playerManaRect.pivot = new Vector2(0.5f, 0.5f);
					playerManaRect.anchoredPosition = isOpponent ? new Vector2(-132f, -10f) : new Vector2(118f, 18f);
					playerManaRect.localScale = Vector3.one * (isOpponent ? 0.76f : 0.86f);
				}

				var opponentMana = actorView.GetComponentInChildren<OpponentManaPanel>(true);
				if (opponentMana && opponentMana.transform is RectTransform opponentManaRect)
				{
					opponentManaRect.anchorMin = new Vector2(0.5f, 0.5f);
					opponentManaRect.anchorMax = new Vector2(0.5f, 0.5f);
					opponentManaRect.pivot = new Vector2(0.5f, 0.5f);
					opponentManaRect.anchoredPosition = new Vector2(-132f, -10f);
					opponentManaRect.localScale = Vector3.one * 0.76f;
				}
			}
		}

		private void ApplyMinimalHud()
		{
			var roundButtonPanel = RoundButtonView.Instance ? FindRect(RoundButtonView.Instance.transform, "ButtonPanel") : null;
			if (roundButtonPanel)
			{
				roundButtonPanel.anchorMin = new Vector2(1f, 0f);
				roundButtonPanel.anchorMax = new Vector2(1f, 0f);
				roundButtonPanel.pivot = new Vector2(1f, 0f);
				roundButtonPanel.anchoredPosition = new Vector2(-68f, 42f);
				roundButtonPanel.localScale = Vector3.one * 0.86f;
			}

			var deckCounter = FindObjectOfType<DeckCounterView>(true)?.transform as RectTransform;
			if (deckCounter)
			{
				deckCounter.anchorMin = new Vector2(1f, 0.5f);
				deckCounter.anchorMax = new Vector2(1f, 0.5f);
				deckCounter.pivot = new Vector2(1f, 0.5f);
				deckCounter.anchoredPosition = new Vector2(-44f, 0f);
			}

			foreach (var graveyardCounter in FindObjectsOfType<GraveyardCounterView>(true))
			{
				if (graveyardCounter.transform is not RectTransform rect)
					continue;

				var isOpponent = graveyardCounter.name.Contains("Opponent");
				rect.anchorMin = new Vector2(1f, isOpponent ? 0.68f : 0.32f);
				rect.anchorMax = rect.anchorMin;
				rect.pivot = new Vector2(1f, 0.5f);
				rect.anchoredPosition = new Vector2(-44f, 0f);
				rect.localScale = Vector3.one * 0.92f;
			}
		}

		private void ApplyMinimalHands()
		{
			var gameView = GameCoreView.Instance;
			if (gameView)
			{
				if (gameView.SelfHandContainer is RectTransform selfHand)
				{
					selfHand.anchorMin = new Vector2(0.5f, 0f);
					selfHand.anchorMax = new Vector2(0.5f, 0f);
					selfHand.pivot = new Vector2(0.5f, 0f);
					selfHand.anchoredPosition = new Vector2(0f, 0f);
					selfHand.sizeDelta = new Vector2(1180f, 250f);
					selfHand.localScale = Vector3.one * 1.06f;
				}

				if (gameView.OpponentHandContainer is RectTransform opponentHand)
				{
					opponentHand.anchorMin = new Vector2(0.5f, 1f);
					opponentHand.anchorMax = new Vector2(0.5f, 1f);
					opponentHand.pivot = new Vector2(0.5f, 1f);
					opponentHand.anchoredPosition = new Vector2(0f, -8f);
					opponentHand.sizeDelta = new Vector2(960f, 140f);
					opponentHand.localScale = Vector3.one * 0.78f;
				}
			}

			foreach (var spline in FindObjectsOfType<HandSpline>(true))
			{
				if (spline.SplineType == SplineType.HandSelf)
				{
					SetSplineArcSize(spline, 2f);
					SetSplineArcScale(spline, new Vector2(320f, 160f));
				}
				else if (spline.SplineType == SplineType.HandOpponent)
				{
					SetSplineArcSize(spline, 1.4f);
					SetSplineArcScale(spline, new Vector2(260f, 90f));
				}
			}
		}

		private void EnsureDividerLine()
		{
			var gameView = GameCoreView.Instance;
			if (!gameView || !gameView.TableContainer)
				return;

			if (!dividerLine)
			{
				dividerLine = new GameObject("MinimalDividerLine", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
				dividerLine.transform.SetParent(gameView.TableContainer, false);
				var image = dividerLine.GetComponent<Image>();
				image.color = new Color(0.95f, 0.75f, 0.2f, 0.4f);
				image.raycastTarget = false;
			}

			var rect = dividerLine.GetComponent<RectTransform>();
			rect.anchorMin = new Vector2(0.1f, 0.5f);
			rect.anchorMax = new Vector2(0.9f, 0.5f);
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.anchoredPosition = Vector2.zero;
			rect.sizeDelta = new Vector2(0f, 3f);
		}

		private void EnsureMinimalSurface(RectTransform table)
		{
			if (!minimalSurface)
			{
				minimalSurface = new GameObject("MinimalSurface", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline));
				minimalSurface.transform.SetParent(table, false);
				var image = minimalSurface.GetComponent<Image>();
				// Dark semi-transparent surface — very different from the classic board
				image.color = new Color(0.02f, 0.02f, 0.04f, 0.82f);
				image.raycastTarget = false;
				// Subtle gold border to frame the play area
				var outline = minimalSurface.GetComponent<Outline>();
				outline.effectColor = new Color(0.85f, 0.65f, 0.15f, 0.5f);
				outline.effectDistance = new Vector2(3f, -3f);
				outline.useGraphicAlpha = false;
			}

			var rect = minimalSurface.GetComponent<RectTransform>();
			rect.anchorMin = new Vector2(0.02f, 0.02f);
			rect.anchorMax = new Vector2(0.98f, 0.98f);
			rect.offsetMin = Vector2.zero;
			rect.offsetMax = Vector2.zero;
			rect.anchoredPosition = Vector2.zero;
			rect.sizeDelta = Vector2.zero;
			rect.SetAsFirstSibling();
		}

		private void RestoreClassicLayout()
		{
			foreach (var (rect, state) in rectStates)
			{
				if (!rect)
					continue;

				rect.anchorMin = state.AnchorMin;
				rect.anchorMax = state.AnchorMax;
				rect.pivot = state.Pivot;
				rect.sizeDelta = state.SizeDelta;
				rect.anchoredPosition = state.AnchoredPosition;
				rect.offsetMin = state.OffsetMin;
				rect.offsetMax = state.OffsetMax;
				rect.localScale = state.LocalScale;
				rect.localRotation = state.LocalRotation;
			}

			foreach (var (graphic, state) in graphicStates)
			{
				if (!graphic)
					continue;

				graphic.enabled = state.Enabled;
				graphic.gameObject.SetActive(state.Active);
			}

			foreach (var (spline, state) in splineStates)
			{
				if (!spline)
					continue;

				SetSplineArcSize(spline, state.ArcSize);
				SetSplineArcScale(spline, state.ArcScale);
			}

			if (dividerLine)
				Destroy(dividerLine);
			dividerLine = null;

			if (minimalSurface)
				Destroy(minimalSurface);
			minimalSurface = null;
		}

		private void OnDestroy()
		{
			if (instance == this)
				instance = null;

			BoardLayoutSettings.Changed -= ApplyCurrentLayout;
			ArenaThemeSettings.Changed -= ApplyCurrentLayout;
		}

		private void CacheRect(RectTransform rect)
		{
			if (!rect || rectStates.ContainsKey(rect))
				return;

			rectStates[rect] = new RectState
			{
				AnchorMin = rect.anchorMin,
				AnchorMax = rect.anchorMax,
				Pivot = rect.pivot,
				SizeDelta = rect.sizeDelta,
				AnchoredPosition = rect.anchoredPosition,
				OffsetMin = rect.offsetMin,
				OffsetMax = rect.offsetMax,
				LocalScale = rect.localScale,
				LocalRotation = rect.localRotation
			};
		}

		private void CacheGraphic(Graphic graphic)
		{
			if (!graphic || graphicStates.ContainsKey(graphic))
				return;

			graphicStates[graphic] = new GraphicState
			{
				Enabled = graphic.enabled,
				Active = graphic.gameObject.activeSelf
			};
		}

		private void CacheSpline(HandSpline spline)
		{
			if (!spline || splineStates.ContainsKey(spline))
				return;

			splineStates[spline] = new SplineState
			{
				ArcSize = GetSplineArcSize(spline),
				ArcScale = GetSplineArcScale(spline)
			};
		}

		private static float GetSplineArcSize(HandSpline spline)
		{
			return ArcSizeField?.GetValue(spline) is float value ? value : 7.6f;
		}

		private static Vector2 GetSplineArcScale(HandSpline spline)
		{
			return ArcScaleField?.GetValue(spline) is Vector2 value ? value : new Vector2(1300f, 777.8f);
		}

		private static void SetSplineArcSize(HandSpline spline, float value)
		{
			ArcSizeField?.SetValue(spline, value);
		}

		private static void SetSplineArcScale(HandSpline spline, Vector2 value)
		{
			ArcScaleField?.SetValue(spline, value);
		}

		private void SetGraphicEnabled(Graphic graphic, bool enabled)
		{
			if (!graphic)
				return;

			CacheGraphic(graphic);
			graphic.enabled = enabled;
			graphic.gameObject.SetActive(enabled);
		}

		private static void StretchRect(RectTransform rect)
		{
			if (!rect)
				return;

			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.one;
			rect.offsetMin = Vector2.zero;
			rect.offsetMax = Vector2.zero;
			rect.anchoredPosition = Vector2.zero;
			rect.sizeDelta = Vector2.zero;
			rect.localScale = Vector3.one;
		}

		private static Graphic FindGraphic(Transform root, string name)
		{
			var target = FindDescendant(root, name);
			return target ? target.GetComponent<Graphic>() : null;
		}

		/// <summary>
		/// Finds a graphic by combining two name parts — tries "Part1Part2", "Part1 Part2", and "Part1_Part2"
		/// to handle prefab naming inconsistencies (e.g. "TableBase" vs "Table Base").
		/// </summary>
		private static Graphic FindGraphicFuzzy(Transform root, string part1, string part2)
		{
			var g = FindGraphic(root, part1 + part2);            // TableBase
			if (g) return g;
			g = FindGraphic(root, part1 + " " + part2);         // Table Base
			if (g) return g;
			g = FindGraphic(root, part1 + "_" + part2);         // Table_Base
			if (g) return g;
			// Also try with trailing space (seen in prefab: 'TopRightCorner ')
			g = FindGraphic(root, part1 + part2 + " ");
			return g;
		}

		private static RectTransform FindRect(Transform root, string name)
		{
			return FindDescendant(root, name) as RectTransform;
		}

		private static Transform FindDescendant(Transform root, string name)
		{
			if (!root)
				return null;

			// Compare with trim to handle trailing spaces in prefab names
			if (root.name == name || root.name.Trim() == name.Trim())
				return root;

			for (var i = 0; i < root.childCount; i++)
			{
				var result = FindDescendant(root.GetChild(i), name);
				if (result)
					return result;
			}

			return null;
		}

		private static IEnumerable<ActorView> GetActorViews()
		{
			return FindObjectsOfType<ActorView>(true)
				.Where(view => view && (view.name.Contains("Actor_Self") || view.name.Contains("Actor_Opponent")));
		}
	}
}
