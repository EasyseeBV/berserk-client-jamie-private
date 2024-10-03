using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Events;
using Game;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using UnityEngine;
using Vulcan.Data;
using Vulcan.Network.Context;
using Vulcan.Network.Resolver;

namespace UI
{
	public partial class MulliganView : BaseView
	{
		[SerializeField] private float fadeDuration = 1f;

		private static readonly List<MulliganCardView> CARD_VIEWS = new();

		private bool done;

		private readonly HandService handService = new HandService(); //TODO: DI

		protected override void OnAwake()
		{
			CanvasGroup.alpha = 0;
			DoneBtn.Subscribe(OnDone);

			GameBus.OnShowMulligan.Subscribe(this, ShowCards).CallWhenInactive();
			ResolverBus.OnMulliganFinished.Subscribe(this, round =>
			{
				GameBus.RoundTimer.Unsubscribe(this);
				StartCoroutine(FinalTickTimerRoutine(round));
			}).CallWhenInactive();
			GameBus.RoundTimer.Subscribe(this, TickTimer);
			GameBus.OnTimerPaused.Subscribe(this, value => DoneBtn.interactable = !value);
		}

		private void ShowCards()
		{
			Show();
			CanvasGroup.blocksRaycasts = true;

			SetActive(YourTurnText, ActorsContextResolver.Self.PlayerIndex == 0);
			SetActive(OpponentTurnText, ActorsContextResolver.Opponent.PlayerIndex == 0);
			
			var handCards = ActorsContextResolver.Self.HandCards;

			CARD_VIEWS.Clear();
			CardsContainer.DestroyChildrenExcept(MulliganCardView.transform);
			MulliganCardView.gameObject.SetActive(true);
			handCards
				.Select(x => x.ToCardData(Berserk.Shared.Data.Enums.Owner.Self))
				.ForEach(data =>
				{
					var cardView = Instantiate(MulliganCardView, CardsContainer.transform).SetUp(data);
					cardView.gameObject.SetActive(true);
					if (ActorsContextResolver.Self.IsMulliganReady)
						cardView.Lock();

					CARD_VIEWS.Add(cardView);
				});
			MulliganCardView.gameObject.SetActive(false);

			CanvasGroup.DOFade(1f, fadeDuration);
		}

		private void OnDone()
		{
			if (done)
				return;

			done = true;
			SetActive(DoneBtn, false);

			CARD_VIEWS.ForEach(x => x.Lock());
			MulliganReadyAsync();
		}

		private async void MulliganReadyAsync()
		{
			// var replacedCards = CARD_VIEWS
			// 	.Where(c => c.Selected)
			// 	.ToArray();
			//
			// var replacedCardIds = replacedCards
			// 	.Select(card => card.Data.UID)
			// 	.ToArray();
			//
			// var response = await GameAPI.ReplaceCardsAsync(replacedCardIds, ActorsContextResolver.Self.Id).ConfigureAwait(true);
			// if (!response.IsSuccess || response.Data == null)
			// 	return;
			//
			// if (replacedCards.Any())
			// {
			// 	var newCards = response.Data
			// 		.Select(x => x.ToCardData(Berserk.Shared.Data.Enums.Owner.Self))
			// 		.ToArray();
			// 	replacedCards.ForEach((cardView, index) => AnimateCard(cardView, newCards.ElementAt(index), index));
			// 	handService.RemoveCards(replacedCards.Select(x => x.Data).ToArray());
			// 	handService.DealCards(newCards);
			// }
		}

		private void AnimateCard(MulliganCardView cardView, CardData newData, int index)
		{
			var localY = cardView.transform.localPosition.y;
			cardView.transform
				.DOLocalMoveY(localY - 1000, 1f)
				.SetDelay(0.1f * index)
				.SetEase(Ease.InCubic)
				.OnComplete(() =>
				{
					cardView.transform
						.DOLocalMoveY(localY, 1f)
						.SetEase(Ease.OutCubic);

					cardView.SetUp(newData);
				});
		}

		private void TickTimer(int value)
		{
			if (value < 0)
			{
				DoneBtn.interactable = false;
				return;
			}

			TimerTxt.SetText($"The game will start in: <size=120%>{value}</size>s");
		}

		private IEnumerator FinalTickTimerRoutine(RoundMessage roundMessage)
		{
			var wait = new WaitForSeconds(1f);
			for (var i = 0; i < 3; i++)
			{
				TimerTxt.SetText($"The game will start in: <size=120%>{3 - i}</size>s");
				yield return wait;
			}

			EndMulligan();

			void EndMulligan()
			{
				CanvasGroup
					.DOFade(0f, fadeDuration)
					.OnComplete(() =>
					{
						CanvasGroup.blocksRaycasts = false;
						RoundResolver.Resolve(roundMessage, roundMessage.UtcTimeStamp);
						Close();
					});

				OnDone();
			}
		}
	}
}