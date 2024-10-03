using Berserk.Shared.Data.Enums;
using Events;
using RR.Core.ResourceManagament;
using RR.Core.Components;
using RR.Core.DebugSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
	//TODO: to BasieView, need refactor - DeckCounterView - now responsible for 2 decks at once: player and opponent
	public class DeckCounterView : Singleton<DeckCounterView>, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler

	{
		[SerializeField] private TextMeshProUGUI counterSelfText;
		[SerializeField] private TextMeshProUGUI counterOpponentText;
		[SerializeField] private Image selfCardImage;
		[SerializeField] private Image opponentCardImage;

		protected override void OnAwake()
		{
			GameBus.OnRequestHandRearrange.Subscribe(this, RefreshCounter);
			GameBus.SpawnCardsInHand.Subscribe(this, RefreshCounter);
			GameBus.OnSpawnCard.Subscribe(this, RefreshCounter);
			GameBus.OnContextUpdated.Subscribe(this, RefreshCounter);
		}

		private void RefreshCounter()
		{
			counterSelfText.SetText($"{GameBus.LocalContext.GetDeckCardCount(Owner.Self)}");
			counterOpponentText.SetText($"{GameBus.LocalContext.GetDeckCardCount(Owner.Opponent)}");
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			var card = GameBus.LocalContext.NextDeckCard;
			if (card != null)
			{
				FullCardView.SetPosition();
				// FullCardView.Show(card);
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			FullCardView.Hide();
		}

		public void OnPointerClick(PointerEventData eventData)
		{
#if UNITY_EDITOR
			RRLogger.Log(GameBus.LocalContext.ToString());
#endif
		}

		private void OnDestroy()
		{
			selfCardImage.ReleaseResource();
			opponentCardImage.ReleaseResource();
		}
	}
}