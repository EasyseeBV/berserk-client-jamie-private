using System.Collections.Generic;
using System.Linq;
using BerserkV3.GameCore.SplineSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameCoreView = BerserkV3.GameCore.UI.GameView;

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
		private bool cached;
		private GameObject dividerLine;

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
			if (BoardLayoutSettings.IsMinimal())
				ApplyCurrentLayout();
		}

		private void ApplyCurrentLayout()
		{
			if (!cached)
				CacheOriginalValues();

			if (BoardLayoutSettings.IsMinimal())
				ApplyMinimalLayout();
			else
				RestoreClassicLayout();
		}

		private void CacheOriginalValues()
		{
			cached = true;

			var background = BackgroundView.Instance;
			if (background)
			{
				CacheRect(background.transform as RectTransform);
				CacheRect(FindRect(background.transform, "BackgroundImage"));
				CacheGraphic(FindGraphic(background.transform, "BackgroundImage"));
				CacheGraphic(FindGraphic(background.transform, "Plane"));
				CacheGraphic(FindGraphic(background.transform, "TableBase"));
				CacheGraphic(FindGraphic(background.transform, "TableBorders"));
				CacheGraphic(FindGraphic(background.transform, "TopLeftCorner"));
				CacheGraphic(FindGraphic(background.transform, "TopRightCorner"));
				CacheGraphic(FindGraphic(background.transform, "BottomLeftCorner"));
				CacheGraphic(FindGraphic(background.transform, "BottomRightCorner"));
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
			var background = BackgroundView.Instance;
			if (!background)
				return;

			StretchRect(background.transform as RectTransform);
			StretchRect(FindRect(background.transform, "BackgroundImage"));

			SetGraphicEnabled(FindGraphic(background.transform, "Plane"), false);
			SetGraphicEnabled(FindGraphic(background.transform, "TableBase"), false);
			SetGraphicEnabled(FindGraphic(background.transform, "TableBorders"), false);
			SetGraphicEnabled(FindGraphic(background.transform, "TopLeftCorner"), false);
			SetGraphicEnabled(FindGraphic(background.transform, "TopRightCorner"), false);
			SetGraphicEnabled(FindGraphic(background.transform, "BottomLeftCorner"), false);
			SetGraphicEnabled(FindGraphic(background.transform, "BottomRightCorner"), false);
		}

		private void ApplyMinimalPlayField()
		{
			var gameView = GameCoreView.Instance;
			if (!gameView || !gameView.TableContainer)
				return;

			var table = gameView.TableContainer;
			table.anchorMin = new Vector2(0.06f, 0.18f);
			table.anchorMax = new Vector2(0.94f, 0.82f);
			table.offsetMin = Vector2.zero;
			table.offsetMax = Vector2.zero;
			table.anchoredPosition = Vector2.zero;
			table.localScale = Vector3.one;

			if (gameView.SelfHeroContainer)
			{
				gameView.SelfHeroContainer.anchorMin = new Vector2(0f, 0f);
				gameView.SelfHeroContainer.anchorMax = new Vector2(0f, 0f);
				gameView.SelfHeroContainer.pivot = new Vector2(0f, 0f);
				gameView.SelfHeroContainer.anchoredPosition = new Vector2(72f, 24f);
			}

			if (gameView.OpponentHeroContainer)
			{
				gameView.OpponentHeroContainer.anchorMin = new Vector2(0.5f, 1f);
				gameView.OpponentHeroContainer.anchorMax = new Vector2(0.5f, 1f);
				gameView.OpponentHeroContainer.pivot = new Vector2(0.5f, 1f);
				gameView.OpponentHeroContainer.anchoredPosition = new Vector2(0f, -18f);
			}
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
					spline.SetArcSize(2f);
					spline.SetArcScale(new Vector2(320f, 160f));
				}
				else if (spline.SplineType == SplineType.HandOpponent)
				{
					spline.SetArcSize(1.4f);
					spline.SetArcScale(new Vector2(260f, 90f));
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

				spline.SetArcSize(state.ArcSize);
				spline.SetArcScale(state.ArcScale);
			}

			if (dividerLine)
				Destroy(dividerLine);
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
				ArcSize = spline.GetArcSize(),
				ArcScale = spline.GetArcScale()
			};
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

		private static RectTransform FindRect(Transform root, string name)
		{
			return FindDescendant(root, name) as RectTransform;
		}

		private static Transform FindDescendant(Transform root, string name)
		{
			if (!root)
				return null;

			if (root.name == name)
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
