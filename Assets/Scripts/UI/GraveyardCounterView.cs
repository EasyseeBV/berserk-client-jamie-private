using UnityEngine;
using Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Berserk.Shared.Data.Enums;
using UnityEngine.UI;
using CardData = Vulcan.Data.CardData;

namespace UI
{
	public class GraveyardCounterView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
	{
#if UNITY_ANDROID || UNITY_IOS
		private static readonly float PRESS_DELAY = .5f;
		private float pressingTime;
		private bool isPressStarted;
		private bool isFingerInBounds;
#endif

		[SerializeField] private List<Image> cards = default;
		[SerializeField] private Owner owner = Owner.None;

		private IEnumerable<CardData> GraveyardCards => GameBus.LocalContext.GetGraveyardCardsByOwner(owner);

		private void Awake()
		{
// 			GameBus.OnEntityDie.Subscribe(this, RefreshCounter);
// 			GameBus.UpdateGraveyard.Subscribe(this, RefreshCounter);
//
// 			RefreshCounter();
//
// #if !UNITY_ANDROID && !UNITY_IOS
// 			GetComponent<Button>().onClick.AddListener(OpenGraveyard);
// #endif
		}

// #if UNITY_ANDROID || UNITY_IOS
// 		private void Update()
// 		{
// 			if (!isFingerInBounds || !isPressStarted)
// 				return;
// 			
// 			var pressDuration = Time.time - pressingTime;
// 			
// 			if (pressDuration <= PRESS_DELAY)
// 				return;
// 			
// 			ShowLastGraveyardCard();
// 		}
// #endif

		public void OnPointerEnter(PointerEventData eventData)
		{
// #if !UNITY_ANDROID && !UNITY_IOS
// 			ShowLastGraveyardCard();
// #else
// 			isFingerInBounds = true;
// #endif
		}

		public void OnPointerExit(PointerEventData eventData)
		{
// #if UNITY_ANDROID || UNITY_IOS
// 			isFingerInBounds = false;
// 			isPressStarted = false;
// #endif
// 			FullCardView.Hide();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
// #if UNITY_ANDROID || UNITY_IOS
// 			pressingTime = Time.time;
// 			isPressStarted = true;
// #endif
		}

		public void OnPointerUp(PointerEventData eventData)
		{
// #if UNITY_ANDROID || UNITY_IOS
// 			if (!isPressStarted)
// 				return;
// 			
// 			isPressStarted = false;
// 			
// 			if (!isFingerInBounds)
// 				return;
//
// 			pressingTime = 0f;
// 			FullCardView.Hide();
// 			OpenGraveyard();
// #endif
		}
		
		private void OpenGraveyard()
		{
			GraveyardView.Instance.InitAndShow(GraveyardCards);
			GameBus.OnBlockUI.Publish(true);
		}

		private void RefreshCounter()
		{
			// var graveyardCardsCount = GraveyardCards.Count();
			// for (var i = 0; i < cards.Count; i++)
			// 	cards[i].gameObject.SetActive(i < graveyardCardsCount);
		}

		private void ShowLastGraveyardCard()
		{
			// var card = ContentRepository.GetCopyOfCard(GraveyardCards.LastOrDefault()?.Id);
			// if (card == null)
			// 	return;
			//
			// FullCardView.SetPosition();
			// FullCardView.Show(card);
		}

		private void OnDestroy()
		{
			// cards.ForEach(cardImage => cardImage.ReleaseResource());
		}
	}
}