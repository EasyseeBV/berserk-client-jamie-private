using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;
using Berserk.Shared.Data.Lobby;
using BerserkV3.Common.DataBase;
using RR.Core.DebugSystem;
using RR.Core.Extensions;

namespace UI
{
	public class DeckCardCollection : IDisposable
	{
		private readonly Dictionary<string, DeckCardStack> collection = new();
		private Func<CardData, bool> filterFunction = _ => true;
		private Func<CardData, int> sortingFunction = _ => 0;

		public delegate void CardUpdatedDelegate(OwnedCard ownedCard);

		public event CardUpdatedDelegate OnCardRemoved;
		public event CardUpdatedDelegate OnCardAdded;
		public event Action<IDeckCardStack> OnStackAdded;
		public event Action<IDeckCardStack> OnStackRemoved;
		public event Action OnCollectionChanged;
		public event Action OnCardSortFilter;

		public int Limit { get; private set; }
		public int Count => collection.Count;
		public int FullCount => collection.Values.Sum(stack => stack.Count);

		public DeckCardCollection(IEnumerable<OwnedCard> cardDataList, int limit = int.MaxValue)
		{
			cardDataList?.ForEach(AddCard);
			Limit = limit;
		}

		public bool CanAdd(CardData cardData)
		{
			return FullCount < Limit
			       && cardData != null
			       && cardData.LimitInDeck > 0
			       && (!collection.ContainsKey(cardData.Id)
			           || collection[cardData.Id].Count < cardData.LimitInDeck);
		}

		public IDeckCardStack[] ToOrdered()
		{
			return collection.Values
				.OrderBy(v => sortingFunction(v.CardData))
				.OfType<IDeckCardStack>()
				.ToArray();
		}

		public List<IDeckCardStack> ToFiltered()
		{
			return collection.Values
				.Where(v => filterFunction(v.CardData))
				.OrderBy(v => sortingFunction(v.CardData))
				.OfType<IDeckCardStack>()
				.ToList();
		}

		public List<IDeckCardStack> GetAll()
		{
			return collection.Values
				.OfType<IDeckCardStack>()
				.ToList();
		}

		public void ApplyFilterFunction(Func<CardData, bool> filter)
		{
			filterFunction = filter;
			OnCardSortFilter?.Invoke();
		}

		public void ApplySortingFunction(SortingType acceptedSorting)
		{
			sortingFunction = acceptedSorting switch
			{
				SortingType.Quadrant => card => ((int)card.Quadrant + 1) * 1000 + GetManaOrder(card),
				SortingType.Rarity => card => (((int)card.Rarity + 1) * 1000) + GetManaOrder(card),
				SortingType.Lava => GetManaOrder,
				_ => throw new NotImplementedException($"Unknown {nameof(SortingType)} : {acceptedSorting}")
			};

			int GetManaOrder(ICardData card)
			{
				return card.SubTypes != null && card.SubTypes.Contains(SubType.Token)
					? -100 + (int)card.Rarity
					: card.Mana;
			}

			OnCardSortFilter?.Invoke();
		}

		public void AddCard(OwnedCard ownedCard)
		{
			var deckCardStack = collection.ContainsKey(ownedCard.CardId)
				? collection[ownedCard.CardId]
				: CreateDeckStack();

			try
			{
				deckCardStack.Add(ownedCard);
				collection[ownedCard.CardId] = deckCardStack;
				OnCardAdded?.Invoke(ownedCard);
				OnStackAdded?.Invoke(deckCardStack);
				OnCollectionChanged?.Invoke();
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}

			DeckCardStack CreateDeckStack()
			{
				var cardData = GameDataBaseAdapter.Instance.GetCard(ownedCard.CardId);
				var deckStack = new DeckCardStack(cardData);
				deckStack.OnRequestToRemove += RemoveCard;
				return deckStack;
			}
		}

		public void RemoveCard(OwnedCard ownedCard)
		{
			if (!collection.TryGetValue(ownedCard.CardId, out var deckCardStack))
			{
				RRLogger.Error($"Could not remove card [{ownedCard.CardId}] from collection");
				return;
			}

			try
			{
				deckCardStack.Remove(ownedCard);

				var disposed = false;
				if (deckCardStack.Count <= 0)
					disposed = collection.Remove(deckCardStack.Id);

				OnCardRemoved?.Invoke(ownedCard);
				OnStackRemoved?.Invoke(deckCardStack);
				if (disposed)
					deckCardStack.Dispose();

				OnCollectionChanged?.Invoke();
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		public bool IsValid()
		{
			return GetAll().All(x => x.AllInStackValid);
		}

		public void Dispose()
		{
			collection.ForEach(x => x.Value?.Dispose());
			collection.Clear();
			filterFunction = null;
			sortingFunction = null;
			OnCardSortFilter = null;
			OnCollectionChanged = null;
			OnStackRemoved = null;
			OnStackAdded = null;
			OnCardRemoved = null;
			OnCardAdded = null;
			Limit = 0;
		}
	}
}