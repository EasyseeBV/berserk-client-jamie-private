using System.Collections.Generic;
using System.Linq;
using BerserkV3.GameCore.UI;
using Cysharp.Threading.Tasks;
using Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public partial class GameTableView
	{
		private sealed class CompactRectState
		{
			public Vector2 AnchorMin;
			public Vector2 AnchorMax;
			public Vector2 Pivot;
			public Vector2 AnchoredPosition;
			public Vector2 SizeDelta;
			public Vector2 OffsetMin;
			public Vector2 OffsetMax;
			public Vector3 LocalScale;
			public Vector3 LocalEulerAngles;
			public int SiblingIndex;
			public bool ActiveSelf;
		}

		private readonly Dictionary<RectTransform, CompactRectState> compactStates = new();
		private readonly Dictionary<string, TextMeshProUGUI> compactBadges = new();

		private RectTransform compactTopBar;
		private TextMeshProUGUI compactTopBarText;
		private TextMeshProUGUI compactOpponentHandBadge;
		private DeckHolderView selfDeckHolder;
		private DeckHolderView opponentDeckHolder;
		private GraveyardPileView selfGraveyardPile;
		private GraveyardPileView opponentGraveyardPile;
		private RectTransform selfHandContainer;
		private RectTransform opponentHandContainer;
		private bool compactBoardInitialized;

		private void InitCompactBoardLayout()
		{
			if (compactBoardInitialized)
				return;

			compactBoardInitialized = true;
			EnsureCompactRuntimeObjects();
			CacheCompactTargets();

			BoardLayoutSettings.Changed += HandleBoardLayoutChanged;

			GameBus.OnContextUpdated.Subscribe(this, RefreshCompactBoardState);
			GameBus.OnSpawnCard.Subscribe(this, _ => RefreshCompactBoardState());
			GameBus.OnRequestHandRearrange.Subscribe(this, _ => RefreshCompactBoardState());
			GameBus.UpdateGraveyard.Subscribe(this, RefreshCompactBoardState);
			GameBus.CurrentRound.Subscribe(this, _ => RefreshCompactBoardState());
			GameBus.RoundTimer.Subscribe(this, _ => RefreshCompactBoardState());

			ApplyBoardLayout(BoardLayoutSettings.IsCompact());
			ForceBoardLayoutSyncAsync().Forget();
		}

		private void OnDestroy()
		{
			BoardLayoutSettings.Changed -= HandleBoardLayoutChanged;
		}

			private void Update()
			{
				if (BoardLayoutSettings.IsCompact())
				{
					ApplyCompactBoardLayout();
					RefreshCompactBoardState();
				}
			}

		private void HandleBoardLayoutChanged()
		{
			ApplyBoardLayout(BoardLayoutSettings.IsCompact());
			ForceBoardLayoutSyncAsync().Forget();
		}

			private async UniTaskVoid ForceBoardLayoutSyncAsync()
			{
				for (var i = 0; i < 6; i++)
				{
					await UniTask.NextFrame();
					if (!this || !gameObject)
						return;

					ApplyBoardLayout(BoardLayoutSettings.IsCompact());
				}
			}

		private void ApplyBoardLayout(bool compact)
		{
			if (!this || !gameObject)
				return;

			EnsureCompactRuntimeObjects();
			CacheCompactTargets();

			if (compact)
			{
				ApplyCompactBoardLayout();
			}
			else
			{
				RestoreClassicBoardLayout();
			}

			RefreshCompactBoardState();
		}

		private void EnsureCompactRuntimeObjects()
		{
			if (!compactTopBar)
			{
				var topBarObject = new GameObject("CompactTopBar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
				compactTopBar = topBarObject.GetComponent<RectTransform>();
				compactTopBar.SetParent(transform, false);
				compactTopBar.anchorMin = new Vector2(0.5f, 1f);
				compactTopBar.anchorMax = new Vector2(0.5f, 1f);
				compactTopBar.pivot = new Vector2(0.5f, 1f);
				compactTopBar.sizeDelta = new Vector2(1680f, 64f);
				compactTopBar.anchoredPosition = new Vector2(0f, -18f);

				var image = topBarObject.GetComponent<Image>();
				image.color = new Color(0.04f, 0.05f, 0.08f, 0.72f);
				image.raycastTarget = false;

				compactTopBarText = CreateCompactText(
					"CompactTopBarText",
					compactTopBar,
					new Vector2(0.5f, 0.5f),
					new Vector2(0.5f, 0.5f),
					Vector2.zero,
					new Vector2(520f, 40f),
					28f,
					TextAlignmentOptions.Center,
					FontStyles.Bold,
					Color.white);

				compactOpponentHandBadge = CreateBadge(
					"CompactOpponentHandBadge",
					compactTopBar,
					new Vector2(1f, 0.5f),
					new Vector2(-44f, 0f),
					new Vector2(116f, 36f),
					18f);
			}

			compactTopBar.gameObject.SetActive(false);
		}

		private void CacheCompactTargets()
		{
			if (Actor_Self)
				CacheRectState((RectTransform)Actor_Self.transform);
			if (Actor_Opponent)
				CacheRectState((RectTransform)Actor_Opponent.transform);
			if (PlayerHandCardPanel)
				CacheRectState(PlayerHandCardPanel);
			if (OpponentHandCardPanel)
				CacheRectState(OpponentHandCardPanel);
			if (!selfHandContainer || !opponentHandContainer)
			{
				var gameView = Object.FindObjectOfType<BerserkV3.GameCore.UI.GameView>(true);
				if (gameView)
				{
					selfHandContainer = gameView.SelfHandContainer as RectTransform;
					opponentHandContainer = gameView.OpponentHandContainer as RectTransform;
				}
			}
			if (selfHandContainer)
				CacheRectState(selfHandContainer);
			if (opponentHandContainer)
				CacheRectState(opponentHandContainer);

			if (RoundButtonView.Instance)
				CacheRectState(RoundButtonView.Instance.GetCompactButtonPanel());

			foreach (var holder in Object.FindObjectsOfType<DeckHolderView>(true))
			{
				if (holder.Owner == Berserk.Shared.Data.Enums.Owner.Self)
					selfDeckHolder = holder;
				else if (holder.Owner == Berserk.Shared.Data.Enums.Owner.Opponent)
					opponentDeckHolder = holder;

				CacheRectState(holder.transform as RectTransform);
			}

			foreach (var pile in Object.FindObjectsOfType<GraveyardPileView>(true))
			{
				if (pile.Owner == Berserk.Shared.Data.Enums.Owner.Self)
					selfGraveyardPile = pile;
				else if (pile.Owner == Berserk.Shared.Data.Enums.Owner.Opponent)
					opponentGraveyardPile = pile;

				CacheRectState(pile.transform as RectTransform);
			}
		}

		private void ApplyCompactBoardLayout()
		{
			if (!compactTopBar)
				return;

			compactTopBar.gameObject.SetActive(true);

			if (Actor_Self)
			{
				var selfActorRect = (RectTransform)Actor_Self.transform;
				SetAnchoredRect(selfActorRect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -12f), new Vector2(360f, 70f), Vector3.one * 0.9f);
			}

			if (Actor_Opponent)
			{
				var opponentActorRect = (RectTransform)Actor_Opponent.transform;
				SetAnchoredRect(opponentActorRect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -12f), new Vector2(360f, 70f), Vector3.one * 0.9f);
			}

			if (OpponentHandCardPanel)
				OpponentHandCardPanel.gameObject.SetActive(false);
			if (opponentHandContainer)
				opponentHandContainer.gameObject.SetActive(false);

			if (RoundButtonView.Instance)
			{
				var buttonPanel = RoundButtonView.Instance.GetCompactButtonPanel();
				SetAnchoredRect(buttonPanel, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-70f, 0f), new Vector2(220f, 140f), Vector3.one * 0.92f);
			}

			ApplyCornerWidgetLayout(selfDeckHolder, new Vector2(1f, 0f), new Vector2(-42f, 82f), "SelfDeckBadge", Berserk.Shared.Data.Enums.Owner.Self, true);
			ApplyCornerWidgetLayout(selfGraveyardPile, new Vector2(0f, 0f), new Vector2(42f, 82f), "SelfGraveBadge", Berserk.Shared.Data.Enums.Owner.Self, false);
			ApplyCornerWidgetLayout(opponentDeckHolder, new Vector2(1f, 1f), new Vector2(-42f, -82f), "OpponentDeckBadge", Berserk.Shared.Data.Enums.Owner.Opponent, true);
			ApplyCornerWidgetLayout(opponentGraveyardPile, new Vector2(0f, 1f), new Vector2(42f, -82f), "OpponentGraveBadge", Berserk.Shared.Data.Enums.Owner.Opponent, false);
		}

		private void RestoreClassicBoardLayout()
		{
			if (Actor_Self)
				RestoreRectState((RectTransform)Actor_Self.transform);
			if (Actor_Opponent)
				RestoreRectState((RectTransform)Actor_Opponent.transform);
			if (PlayerHandCardPanel)
				RestoreRectState(PlayerHandCardPanel);
			if (OpponentHandCardPanel)
				RestoreRectState(OpponentHandCardPanel);
			if (selfHandContainer)
				RestoreRectState(selfHandContainer);
			if (opponentHandContainer)
				RestoreRectState(opponentHandContainer);

			if (RoundButtonView.Instance)
				RestoreRectState(RoundButtonView.Instance.GetCompactButtonPanel());

			if (selfDeckHolder)
				RestoreRectState(selfDeckHolder.transform as RectTransform);
			if (opponentDeckHolder)
				RestoreRectState(opponentDeckHolder.transform as RectTransform);
			if (selfGraveyardPile)
				RestoreRectState(selfGraveyardPile.transform as RectTransform);
			if (opponentGraveyardPile)
				RestoreRectState(opponentGraveyardPile.transform as RectTransform);

			foreach (var badge in compactBadges.Values)
				badge.transform.parent.gameObject.SetActive(false);

			if (compactTopBar)
				compactTopBar.gameObject.SetActive(false);

			if (OpponentHandCardPanel)
				OpponentHandCardPanel.gameObject.SetActive(true);
			if (opponentHandContainer)
				opponentHandContainer.gameObject.SetActive(true);
		}

		private void RefreshCompactBoardState()
		{
			if (!compactTopBar || !compactTopBarText || !compactOpponentHandBadge)
				return;

			if (!BoardLayoutSettings.IsCompact())
				return;

			var currentRound = GameBus.CurrentRound?.Value;
			if (currentRound == null || GameBus.LocalContext == null)
				return;

			var turnText = currentRound.TurnOwner == Berserk.Shared.Data.Enums.Owner.Self
				? "YOUR TURN"
				: currentRound.TurnOwner == Berserk.Shared.Data.Enums.Owner.Opponent
					? "ENEMY TURN"
					: "WAITING";

			compactTopBarText.text = $"ROUND {Mathf.Max(1, GameBus.LocalContext.RoundNumber)}  •  {turnText}  •  {Mathf.Max(0, GameBus.RoundTimer.Value)}s";
			compactOpponentHandBadge.text = $"CARDS {Mathf.Max(0, GameBus.LocalContext.GetHandCardsCount(Berserk.Shared.Data.Enums.Owner.Opponent))}";

			UpdateBadge("SelfDeckBadge", GameBus.LocalContext.GetDeckCardCount(Berserk.Shared.Data.Enums.Owner.Self).ToString());
			UpdateBadge("OpponentDeckBadge", GameBus.LocalContext.GetDeckCardCount(Berserk.Shared.Data.Enums.Owner.Opponent).ToString());
			UpdateBadge("SelfGraveBadge", GameBus.LocalContext.GetGraveyardCardsByOwner(Berserk.Shared.Data.Enums.Owner.Self).Count().ToString());
			UpdateBadge("OpponentGraveBadge", GameBus.LocalContext.GetGraveyardCardsByOwner(Berserk.Shared.Data.Enums.Owner.Opponent).Count().ToString());
		}

		private void ApplyCornerWidgetLayout(Component component, Vector2 anchor, Vector2 position, string badgeKey, Berserk.Shared.Data.Enums.Owner owner, bool isDeck)
		{
			if (!component)
				return;

			var rect = component.transform as RectTransform;
			if (!rect)
				return;

			SetAnchoredRect(rect, anchor, anchor, new Vector2(0.5f, 0.5f), position, new Vector2(52f, 70f), Vector3.one * 0.42f);
			rect.SetAsLastSibling();

			var badge = GetOrCreateBadge(badgeKey, rect);
			badge.transform.parent.gameObject.SetActive(true);
			badge.text = isDeck
				? GameBus.LocalContext.GetDeckCardCount(owner).ToString()
				: GameBus.LocalContext.GetGraveyardCardsByOwner(owner).Count().ToString();
		}

		private void CacheRectState(RectTransform rect)
		{
			if (!rect || compactStates.ContainsKey(rect))
				return;

			compactStates[rect] = new CompactRectState
			{
				AnchorMin = rect.anchorMin,
				AnchorMax = rect.anchorMax,
				Pivot = rect.pivot,
				AnchoredPosition = rect.anchoredPosition,
				SizeDelta = rect.sizeDelta,
				OffsetMin = rect.offsetMin,
				OffsetMax = rect.offsetMax,
				LocalScale = rect.localScale,
				LocalEulerAngles = rect.localEulerAngles,
				SiblingIndex = rect.GetSiblingIndex(),
				ActiveSelf = rect.gameObject.activeSelf
			};
		}

		private void RestoreRectState(RectTransform rect)
		{
			if (!rect || !compactStates.TryGetValue(rect, out var state))
				return;

			rect.anchorMin = state.AnchorMin;
			rect.anchorMax = state.AnchorMax;
			rect.pivot = state.Pivot;
			rect.anchoredPosition = state.AnchoredPosition;
			rect.sizeDelta = state.SizeDelta;
			rect.offsetMin = state.OffsetMin;
			rect.offsetMax = state.OffsetMax;
			rect.localScale = state.LocalScale;
			rect.localEulerAngles = state.LocalEulerAngles;
			rect.SetSiblingIndex(Mathf.Min(state.SiblingIndex, rect.parent ? rect.parent.childCount - 1 : state.SiblingIndex));
			rect.gameObject.SetActive(state.ActiveSelf);
		}

		private static void SetAnchoredRect(
			RectTransform rect,
			Vector2 anchorMin,
			Vector2 anchorMax,
			Vector2 pivot,
			Vector2 anchoredPosition,
			Vector2 sizeDelta,
			Vector3 scale)
		{
			if (!rect)
				return;

			rect.gameObject.SetActive(true);
			rect.anchorMin = anchorMin;
			rect.anchorMax = anchorMax;
			rect.pivot = pivot;
			rect.anchoredPosition = anchoredPosition;
			rect.sizeDelta = sizeDelta;
			rect.localScale = scale;
			rect.localEulerAngles = Vector3.zero;
		}

		private TextMeshProUGUI GetOrCreateBadge(string key, RectTransform target)
		{
			if (compactBadges.TryGetValue(key, out var existing))
				return existing;

			var badge = CreateBadge(key, target, new Vector2(1f, 1f), new Vector2(-10f, -8f), new Vector2(34f, 28f), 16f);
			compactBadges[key] = badge;
			return badge;
		}

		private void UpdateBadge(string key, string value)
		{
			if (compactBadges.TryGetValue(key, out var badge))
				badge.text = value;
		}

		private static TextMeshProUGUI CreateCompactText(
			string name,
			Transform parent,
			Vector2 anchorMin,
			Vector2 anchorMax,
			Vector2 anchoredPosition,
			Vector2 size,
			float fontSize,
			TextAlignmentOptions alignment,
			FontStyles style,
			Color color)
		{
			var textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
			var rect = textObject.GetComponent<RectTransform>();
			rect.SetParent(parent, false);
			rect.anchorMin = anchorMin;
			rect.anchorMax = anchorMax;
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.anchoredPosition = anchoredPosition;
			rect.sizeDelta = size;

			var text = textObject.GetComponent<TextMeshProUGUI>();
			text.fontSize = fontSize;
			text.alignment = alignment;
			text.fontStyle = style;
			text.color = color;
			text.raycastTarget = false;
			text.enableWordWrapping = false;
			return text;
		}

		private static TextMeshProUGUI CreateBadge(
			string name,
			Transform parent,
			Vector2 anchor,
			Vector2 anchoredPosition,
			Vector2 size,
			float fontSize)
		{
			var root = new GameObject($"{name}_Root", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
			var rootRect = root.GetComponent<RectTransform>();
			rootRect.SetParent(parent, false);
			rootRect.anchorMin = anchor;
			rootRect.anchorMax = anchor;
			rootRect.pivot = new Vector2(0.5f, 0.5f);
			rootRect.anchoredPosition = anchoredPosition;
			rootRect.sizeDelta = size;

			var bg = root.GetComponent<Image>();
			bg.color = new Color(0.08f, 0.08f, 0.11f, 0.9f);
			bg.raycastTarget = false;

			var text = CreateCompactText(name, root.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size, fontSize, TextAlignmentOptions.Center, FontStyles.Bold, new Color(1f, 0.92f, 0.64f, 1f));
			return text;
		}
	}
}
