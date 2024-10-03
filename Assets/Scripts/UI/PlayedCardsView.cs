using Events;
using RR.UI.FrameSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vulcan.Data;

namespace UI
{
	public partial class PlayedCardsView : BaseView
	{
		private Queue<CardData> playedCardsData = new Queue<CardData>();

		private bool isShown = false;

		protected override void OnAwake()
		{
			GameBus.OnSpawnCard.Subscribe(this, (cardData) => cardData.Owner == Berserk.Shared.Data.Enums.Owner.Opponent, CreateCard);
			PlayerHandCard.Init();
		}

		private void ShowCards()
		{
			isShown = true;
			StartCoroutine(ShowCardsCoroutine(playedCardsData));
		}

		private IEnumerator ShowCardsCoroutine(Queue<CardData> playedCardsData)
		{
			while (playedCardsData.Count > 0)
			{
				var data = playedCardsData.Dequeue();

				PlayerHandCard.UpdateValues(data)
					.SetDefaultPosition();

				PlayerHandCard.Show();

				yield return new WaitForSeconds(2f);
			}

			PlayerHandCard.Close();
			isShown = false;
		}

		private void CreateCard(CardData data)
		{
			playedCardsData.Enqueue(data);
			if (!isShown)
			{
				ShowCards();
			}
		}
	}
}