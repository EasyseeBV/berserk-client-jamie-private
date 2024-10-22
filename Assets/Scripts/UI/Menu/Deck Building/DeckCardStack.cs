using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Game;
using Berserk.Shared.Data.UserInventory;
using Berserk.Shared.GameCore.Utils;

namespace UI
{
	public interface IDeckCardStack : IDisposable
	{
		string Id { get; }
		int Count { get; }
		bool AnyInStackValid { get; }
		bool AllInStackValid { get; }
		CardData CardData { get; }
		event Action<IDeckCardStack> OnStackChanged;

		OwnedCard[] GetAll();
		OwnedCard Get(Func<OwnedCard[], OwnedCard> selector = null);
		bool ValidateCard(OwnedCard ownedCard);
		void RequestToRemove(OwnedCard ownedCard);
	}

	public class DeckCardStack : IDeckCardStack
	{
		private readonly List<OwnedCard> stack;

		public string Id => CardData?.Id;
		public int Count => stack.Count;
		public CardData CardData { get; private set; }
		public event Action<OwnedCard> OnRequestToRemove;
		public event Action<IDeckCardStack> OnStackChanged;
		public bool AnyInStackValid => stack.Any(ValidateCard);
		public bool AllInStackValid => stack.All(ValidateCard);

		public DeckCardStack(CardData cardData)
		{
			if (cardData == null)
				throw new ArgumentException($"Cant initialize {nameof(DeckCardStack)} : " +
				                            $"{nameof(CardData)} is missing");

			CardData = cardData;
			stack = new List<OwnedCard>();
		}

		public OwnedCard[] GetAll()
		{
			return stack.ToArray();
		}

		public OwnedCard Get(Func<OwnedCard[], OwnedCard> selector = null)
		{
			return selector != null
				? selector.Invoke(stack.ToArray())
				: stack.FirstOrDefault(ValidateCard);
		}
		
		public bool ValidateCard(OwnedCard ownedCard)
		{
			return ownedCard.IsValid();
		}

		public void RequestToRemove(OwnedCard ownedCard)
		{
			OnRequestToRemove?.Invoke(ownedCard);
		}

		public void Add(OwnedCard ownedCard)
		{
			if (ownedCard == null || ownedCard.CardId != Id)
				throw new ArgumentException($"Cant stack : [{ownedCard?.CardId}] " +
				                            $"because the cards are different : {Id}");

			if (stack.Contains(ownedCard))
				throw new ArgumentException($"Cant stack : same card twice : {ownedCard.Id}");

			stack.Add(ownedCard);
			OnStackChanged?.Invoke(this);
		}

		public void Remove(OwnedCard ownedCard)
		{
			if (ownedCard == null || ownedCard.CardId != Id)
				throw new ArgumentException($"Cant remove form stack : [{ownedCard?.CardId}] " +
				                            $"because the cards are different : {Id}");

			if (!stack.Remove(ownedCard))
				throw new ArgumentException($"Cant remove form stackId {Id}, : {ownedCard.Id}");

			OnStackChanged?.Invoke(this);
		}

		public void Dispose()
		{
			OnStackChanged = null;
			OnRequestToRemove = null;
			CardData = null;
			stack.Clear();
		}
	}
}