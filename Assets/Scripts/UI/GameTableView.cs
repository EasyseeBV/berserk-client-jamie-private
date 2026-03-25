using Audio;
using BerserkV3.Common.Network;
using BerserkV3.GameCore.Network;
using BerserkV3.Startup.Network;
using DG.Tweening;
using Events;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using UnityEngine;
using Vulcan.Audio;
using Vulcan.Data;
using Vulcan.Network;
using Vulcan.Network.Context;
using Vulcan.Network.Resolver;

namespace UI
{
	public partial class GameTableView : BaseView
	{
		private CanvasGroup yourTurnCvG;
		private Sequence turnAnimationTween;

		protected override void Start()
		{
			base.Start();
			GameTableViewMinimal.EnsureInitialized();
			YourTurnPanelOverlay.gameObject.SetActive(false);
			yourTurnCvG = YourTurnPanelOverlay.GetComponent<CanvasGroup>();

			GameBus.CurrentRound.Subscribe(this, OnNextRound);
			GameBus.OnVulcaniteDies.Subscribe(this, OnActorDies);

			SettingsBtn.Subscribe(() => SettingsView.Instance.Show());
			VersionTxt.SetText(EnvironmentSwitcher.GetCurrentVersion());

			ResolverBus.OnMulliganFinished.Subscribe(this, ShowCommend);
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
	}
}
