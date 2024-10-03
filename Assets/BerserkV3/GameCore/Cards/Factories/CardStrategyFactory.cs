using System;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.RuntimeObjects;
using Zenject;

namespace BerserkV3.GameCore.Cards
{
	public class CardStrategyFactory : ICardStrategyFactory
	{
		private readonly IInstantiator instantiator;

		public CardStrategyFactory(IInstantiator instantiator)
		{
			this.instantiator = instantiator;
		}

		public ICardStrategy Create(RuntimeState state, ICardView view)
		{
			switch (state)
			{
				case RuntimeState.InChoose:  return Create<CardChooseStrategy>(view);
				case RuntimeState.InTable:   return GetTableStartegyByType(view);
				case RuntimeState.InDiscard: return Create<CardDiscardStrategy>(view);
				case RuntimeState.InHand:    return Create<CardHandStrategy>(view);
				case RuntimeState.InShowAll: return Create<CardInShowAllStrategy>(view);
				case RuntimeState.InShow:    return Create<CardInShowStrategy>(view);
				
				case RuntimeState.InDeck:
				case RuntimeState.InExile:
				default:                         return Create<CardDeckStrategy>(view);
			}
		}

		private ICardStrategy GetTableStartegyByType(ICardView view)
		{
			return view.RuntimeGameObject.Data.Type switch
			{
				ObjectType.Building => Create<CardInTableStartegy>(view),
				_ => Create<CardCreatureStrategy>(view)
			};
		}

		private T Create<T>(params object[] args)
		{
			return instantiator.Instantiate<T>(args);
		}
	}
}