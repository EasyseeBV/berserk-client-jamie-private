using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.Models;
using Berserk.Shared.GameCore.RuntimeObjects;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.Cards;
using BerserkV3.GameCore.LogicEventsProcessor;
using BerserkV3.GameCore.Repository;
using RR.Core.Extensions;
using UI;
using Zenject;
using Object = UnityEngine.Object;

namespace BerserkV3.GameCore.Controllers
{
	public class OpponentHandController : DisposableWithCts, IInitializable
	{
		private readonly IOpponentHandViewFactory cardViewFactory;
		private readonly ICardsStateMachine cardsStateMachine;
		private readonly IGameLogicEventsSource gameLogicEventsSource;
		private readonly IGameRepository gameRepository;
		private readonly List<ICardView> existedCards;
		private IRuntimeGameCard mockRuntimeObject;

		public OpponentHandController(
			IOpponentHandViewFactory cardViewFactory,
			ICardsStateMachine cardsStateMachine,
			IGameLogicEventsSource gameLogicEventsSource,
			IGameRepository gameRepository)
		{
			this.cardViewFactory = cardViewFactory;
			this.cardsStateMachine = cardsStateMachine;
			this.gameLogicEventsSource = gameLogicEventsSource;
			this.gameRepository = gameRepository;
			existedCards = new List<ICardView>();
		}

		public override void Dispose()
		{
			base.Dispose();
			existedCards.ForEach(x =>
			{
				if (x == null || !x.SelfContainer.Value())
					return;

				Object.Destroy(x.SelfContainer.gameObject);
			});
			existedCards.Clear();
			mockRuntimeObject?.Dispose();
			mockRuntimeObject = null;
		}

		public void Initialize()
		{
			gameLogicEventsSource.Subscribe<ChangeHandCount>(data => Evaluate(data.HandCount), Token);
			gameLogicEventsSource.Subscribe<InitializeGame>(InitializeFakeData, Token);
		}

		private void InitializeFakeData()
		{
			var mockData = new CardData();
			var runtimeCard = new RuntimeGameCard();
			var mockRuntimeData = new RuntimeCardData(mockData)
			{
				Id = int.MinValue,
				OwnerUserId = gameRepository.OpponentId,
			};
			mockRuntimeObject = runtimeCard;
			runtimeCard.Init(mockRuntimeData, mockData);
			mockRuntimeObject.UpdateState(RuntimeState.InHand);
		}
		
			private void Evaluate(int count)
			{
				if (BoardLayoutSettings.IsCompact())
				{
					foreach (var cardView in existedCards.ToArray())
					{
						existedCards.Remove(cardView);
						if (cardView?.SelfContainer != null && cardView.SelfContainer.Value())
							Object.Destroy(cardView.SelfContainer.gameObject);
					}

					return;
				}

				if (count < existedCards.Count)
				{
				var removeCount = existedCards.Count - count;
				var remove = existedCards.TakeLast(removeCount).ToArray();
				foreach (var cardView in remove)
				{
					existedCards.Remove(cardView);
					Object.Destroy(cardView.SelfContainer.gameObject);
				}
			}

			if (count > existedCards.Count)
			{
				var createCount = count - existedCards.Count;
				for (var i = 0; i < createCount; i++)
				{
					var cardView = cardViewFactory.Create(mockRuntimeObject);
					if (cardView == null)
						throw new NullReferenceException($"{nameof(ICardViewFactory)} card view does not created!");

					existedCards.Add(cardView);
				}
			}

			if (existedCards.Count <= 0)
				return;

			cardsStateMachine.SetupCardsInHand(existedCards);
			cardsStateMachine.RearrangeHandCardsAsync(false, Owner.Opponent, existedCards.ToArray());
		}
	}
}
