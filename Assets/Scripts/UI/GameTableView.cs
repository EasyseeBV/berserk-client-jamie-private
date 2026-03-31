using Audio;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.Data.Campaign;
using BerserkV3.Common.Network;
using BerserkV3.GameCore.Network;
using BerserkV3.Startup.Network;
using BerserkV3.Lobby.Network;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Events;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vulcan.Audio;
using Vulcan.Data;
using Vulcan.Network;
using Vulcan.Network.Context;
using Vulcan.Network.Resolver;
using Zenject;

namespace UI
{
	public partial class GameTableView : BaseView
	{
		[Inject] private IGameContext gameContext;
		private CanvasGroup yourTurnCvG;
		private Sequence turnAnimationTween;
		private RectTransform campaignTreasureHud;

		protected override void Start()
		{
			base.Start();
			GameTableViewMinimal.EnsureInitialized();
			YourTurnPanelOverlay.gameObject.SetActive(false);
			yourTurnCvG = YourTurnPanelOverlay.GetComponent<CanvasGroup>();

			GameBus.CurrentRound.Subscribe(this, OnNextRound);
			GameBus.OnVulcaniteDies.Subscribe(this, OnActorDies);

			SettingsBtn.Subscribe(ShowSettings);
			VersionTxt.SetText(EnvironmentSwitcher.GetCurrentVersion());

			ResolverBus.OnMulliganFinished.Subscribe(this, ShowCommend);
			LoadCampaignTreasureHudAsync().Forget();
		}

		private static void ShowSettings()
		{
			var view = SettingsView.Instance;
			if (view == null)
			{
				var views = Resources.FindObjectsOfTypeAll<SettingsView>();
				if (views != null && views.Length > 0)
					view = views[0];
			}

			if (view != null)
				view.Show();
		}

		private async void ShowCommend()
		{
			// ResolverBus.OnMulliganFinished.Unsubscribe(this);
			// var isBot = ActorsContextResolver.Opponent.IsControlledByAI;
			//
			// SetActive(CommendLevel, !isBot);
			// if (isBot)
			// 	return;
			//
			// var value = await GameAPI.GetUserCommends() ?? "0";
			// Set(CommendLevel, $"COMMENDATION LEVEL {value}");
		}

		private void OnActorDies(DataBase obj)
		{
			turnAnimationTween?.Kill();
			yourTurnCvG.alpha = 0;
		}

		private void OnNextRound(RoundData round)
		{
			if (CommendLevel.color.a > 0)
				CommendLevel.DOFade(0f, 5f);

			if (round.TurnOwner == Berserk.Shared.Data.Enums.Owner.Self)
				ShowTurnAnimation();
		}

		private void ShowTurnAnimation()
		{
			AudioController.Play(Clip.YourTurn);
			YourTurnPanelOverlay.gameObject.SetActive(true);

			turnAnimationTween?.Kill();
			yourTurnCvG.alpha = 0;

			turnAnimationTween = DOTween.Sequence();
			turnAnimationTween.Append(yourTurnCvG.DOFade(1, 0.5f).SetEase(Ease.InOutQuad));
			turnAnimationTween.Append(yourTurnCvG.DOFade(0, 0.5f).SetEase(Ease.InOutQuad).SetDelay(0.5f));
		}

		private async UniTaskVoid LoadCampaignTreasureHudAsync()
		{
			await UniTask.WaitUntil(() => gameContext?.RuntimeData != null, cancellationToken: this.GetCancellationTokenOnDestroy());

			if (gameContext?.RuntimeData?.MatchMode != MatchMode.Campaign)
				return;

			try
			{
				var response = await CampaignAPI.GetProgress();
				if (!response.IsSuccess || response.Data == null)
					return;

				BuildCampaignTreasureHud(response.Data);
			}
			catch
			{
				// Campaign HUD is a convenience layer and should never interrupt match startup.
			}
		}

		private void BuildCampaignTreasureHud(CampaignProgressModel progress)
		{
			HideCampaignTreasureHud();

			var activeTreasures = progress?.ActiveTreasures;
			if (activeTreasures == null || activeTreasures.Count == 0)
				return;

			var redirect = BerserkV3.Startup.Authorization.User.GetRedirections<CampaignRedirectArg>().FirstOrDefault();
			var quadrantLabel = redirect.Quadrant == 0 ? "CAMPAIGN" : redirect.Quadrant.ToString().Replace("_", " ").ToUpperInvariant();
			var modeLabel = redirect.IsHeroic ? "HEROIC" : "NORMAL";
			var visibleCount = Mathf.Min(activeTreasures.Count, 3);
			var hudHeight = 84f + visibleCount * 26f;

			campaignTreasureHud = CreateRect("CampaignTreasureHud", transform as RectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
				new Vector2(0f, 1f), new Vector2(22f, -22f), new Vector2(286f, hudHeight));
			var hudImage = campaignTreasureHud.gameObject.AddComponent<Image>();
			hudImage.color = new Color(0.05f, 0.06f, 0.08f, 0.88f);
			var hudOutline = campaignTreasureHud.gameObject.AddComponent<Outline>();
			hudOutline.effectDistance = new Vector2(2f, -2f);
			hudOutline.effectColor = new Color(0.96f, 0.72f, 0.18f, 0.72f);

			var title = CreateText("CampaignTreasureHudTitle", campaignTreasureHud, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
				new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(248f, 22f), 16f, "ACTIVE TREASURES");
			title.alignment = TextAlignmentOptions.Center;
			title.color = new Color(0.99f, 0.84f, 0.42f, 1f);
			title.fontStyle = FontStyles.Bold;

			var meta = CreateText("CampaignTreasureHudMeta", campaignTreasureHud, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
				new Vector2(0.5f, 1f), new Vector2(0f, -38f), new Vector2(252f, 18f), 10.5f, $"{quadrantLabel}  •  {modeLabel}");
			meta.alignment = TextAlignmentOptions.Center;
			meta.color = new Color(0.86f, 0.88f, 0.9f, 0.9f);

			for (var i = 0; i < visibleCount; i++)
			{
				CreateTreasureHudRow(campaignTreasureHud, activeTreasures[i], i);
			}

			if (activeTreasures.Count > visibleCount)
			{
				var footer = CreateText("CampaignTreasureHudMore", campaignTreasureHud, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
					new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(248f, 16f), 9.5f, $"+{activeTreasures.Count - visibleCount} more campaign treasures active");
				footer.alignment = TextAlignmentOptions.Center;
				footer.color = new Color(0.9f, 0.82f, 0.58f, 0.85f);
			}
		}

		private void CreateTreasureHudRow(RectTransform parent, CampaignTreasureChoiceModel treasure, int index)
		{
			var row = CreateRect($"TreasureHudRow_{index}", parent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
				new Vector2(0.5f, 1f), new Vector2(0f, -68f - index * 26f), new Vector2(250f, 22f));
			var rowImage = row.gameObject.AddComponent<Image>();
			rowImage.color = new Color(0.16f, 0.12f, 0.06f, 0.92f);

			var title = CreateText("TreasureHudRowTitle", row, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
				new Vector2(8f, 0f), new Vector2(150f, 16f), 10.5f, treasure.Title.ToUpperInvariant());
			title.fontStyle = FontStyles.Bold;
			title.color = new Color(0.98f, 0.86f, 0.48f, 1f);

			var bonus = CreateText("TreasureHudRowBonus", row, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
				new Vector2(-8f, 0f), new Vector2(118f, 16f), 9.5f, BuildTreasureHudBonusText(treasure));
			bonus.alignment = TextAlignmentOptions.Right;
			bonus.color = new Color(0.93f, 0.95f, 0.97f, 0.92f);
		}

		private static string BuildTreasureHudBonusText(CampaignTreasureChoiceModel treasure)
		{
			if (treasure == null)
				return string.Empty;

			if (treasure.BonusHeroHp > 0 && treasure.BonusCards > 0)
				return $"+{treasure.BonusHeroHp} HP  •  +{treasure.BonusCards} card";

			if (treasure.BonusHeroHp > 0)
				return $"+{treasure.BonusHeroHp} HP";

			if (treasure.BonusCards > 0)
				return $"+{treasure.BonusCards} card";

			return "ACTIVE";
		}

		private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 size)
		{
			var go = new GameObject(name, typeof(RectTransform));
			go.transform.SetParent(parent, false);
			var rect = go.GetComponent<RectTransform>();
			rect.anchorMin = anchorMin;
			rect.anchorMax = anchorMax;
			rect.pivot = pivot;
			rect.anchoredPosition = anchoredPosition;
			rect.sizeDelta = size;
			rect.localScale = Vector3.one;
			return rect;
		}

		private static TextMeshProUGUI CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
			Vector2 anchoredPosition, Vector2 size, float fontSize, string text)
		{
			var rect = CreateRect(name, parent, anchorMin, anchorMax, pivot, anchoredPosition, size);
			var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
			label.fontSize = fontSize;
			label.text = text ?? string.Empty;
			label.enableWordWrapping = false;
			label.overflowMode = TextOverflowModes.Ellipsis;
			label.color = Color.white;
			return label;
		}

		private void HideCampaignTreasureHud()
		{
			if (campaignTreasureHud == null)
				return;

			Destroy(campaignTreasureHud.gameObject);
			campaignTreasureHud = null;
		}
	}
}
