using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Berserk.Shared.Data.Enums;
using Game.Entities;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using ServerCore.Infrastructure.Models;
using CardData = Vulcan.Data.CardData;

namespace Events
{
	/// <summary>
	///     Public game context, fairly available to both players.
	/// </summary>
	public class LocalContext
	{
		public event Action<Owner> OnTableChanged;

		private readonly Dictionary<Owner, ActorEntity> vulcanites = new Dictionary<Owner, ActorEntity>
		{
			{ Owner.Self, null },
			{ Owner.Opponent, null }
		};

		private readonly Dictionary<Owner, int> deckCardsCount = new Dictionary<Owner, int>
		{
			{ Owner.Self, 0 },
			{ Owner.Opponent, 0 }
		};

		private readonly Dictionary<Owner, List<TableCardEntity>> tableCards = new Dictionary<Owner, List<TableCardEntity>>
		{
			{ Owner.Self, new List<TableCardEntity>() },
			{ Owner.Opponent, new List<TableCardEntity>() }
		};

		private readonly Dictionary<Owner, List<HandCardEntity>> handCards = new Dictionary<Owner, List<HandCardEntity>>
		{
			{ Owner.Self, new List<HandCardEntity>() },
			{ Owner.Opponent, new List<HandCardEntity>() }
		};

		private readonly Dictionary<Owner, List<CardData>> graveyardCards = new Dictionary<Owner, List<CardData>>
		{
			{ Owner.Self, new List<CardData>() },
			{ Owner.Opponent, new List<CardData>() }
		};

		public string SessionId { get; set; }
		public DateTime StartDateTime { get; set; }
		public int RoundNumber { get; set; }
		public bool RoundAccepted { get; set; }
		public bool UseLeaderboardStatistics { get; set; }
		public bool GameEnded { get; set; }
		public long TimeStamp { get; set; }
		public CardData NextDeckCard { get; private set; }
		public DateTime TurnTimerEndsOn { get; protected set; } = DateTime.MinValue;
		public SessionState State { get; set; }
		public IEnumerable<ActorEntity> Vulcanites => vulcanites.Values.ToArray();

		public IEnumerable<TableCardEntity> TableCardsList => tableCards.Values.SelectMany(x => x).ToList();

		public IEnumerable<HandCardEntity> HandCardsList => handCards.Values.SelectMany(x => x).ToList();

		public IEnumerable<CardData> GraveyardCardsList => graveyardCards.Values.SelectMany(x => x).ToList();

		public void AssignNextDeckCard(CardData cardData)
		{
			NextDeckCard = cardData;
		}

		public IEnumerable<HandCardEntity> GetHandCardsByOwner(Owner owner)
		{
			return handCards.ContainsKey(owner)
				? handCards[owner]
				: new List<HandCardEntity>();
		}

		public ActorEntity GetVulcaniteByOwner(Owner owner)
		{
			return vulcanites.ContainsKey(owner)
				? vulcanites[owner]
				: default;
		}

		public IEnumerable<TableCardEntity> GetTableCardsByOwner(Owner owner)
		{
			return tableCards.ContainsKey(owner)
				? tableCards[owner]
				: new List<TableCardEntity>();
		}

		public IEnumerable<TableCardEntity> GetAliveTableCardsByOwner(Owner owner)
		{
			return tableCards.ContainsKey(owner)
				? tableCards[owner].Where(c => c.Data?.Hp > 0)
				: new List<TableCardEntity>();
		}

		public IEnumerable<CardData> GetGraveyardCardsByOwner(Owner owner)
		{
			return graveyardCards.ContainsKey(owner)
				? graveyardCards[owner]
				: new List<CardData>();
		}

		public IEnumerable<IInteractiveEntity> GetOwnerEntities(Owner owner)
		{
			var result = new List<IInteractiveEntity>();
			result.AddRange(GetTableCardsByOwner(owner));
			var vulcanite = GetVulcaniteByOwner(owner);
			if (vulcanite != null)
				result.Add(vulcanite);
			return result;
		}

		public IEnumerable<IInteractiveEntity> GetAllTargetEntities()
		{
			var result = new List<IInteractiveEntity>();
			result.AddRange(GetTableCardsByOwner(Owner.Self));
			result.AddRange(GetTableCardsByOwner(Owner.Opponent));
			result.AddRange(vulcanites?.Values.ToArray() ?? Array.Empty<ActorEntity>());
			return result;
		}

		public void AddVulcanite(ActorEntity vulcanite)
		{
			if (vulcanite != null && vulcanites.ContainsKey(vulcanite.Owner))
				vulcanites[vulcanite.Owner] = vulcanite;
		}

		public void AddGraveyardCard(CardData data)
		{
			if (data != null && graveyardCards.ContainsKey(data.Owner))
				graveyardCards[data.Owner].Add(data);
		}

		public void AddHandCard(HandCardEntity handCardEntity)
		{
			if (handCardEntity != null
			    && handCards.ContainsKey(handCardEntity.Owner))
				handCards[handCardEntity.Owner].Add(handCardEntity);
		}

		public int AddGraveyardCards(Owner owner, IEnumerable<CardData> graveCards)
		{
			if (!graveyardCards.ContainsKey(owner) || graveCards == null) 
				return 0;
			
			var oldCount = graveyardCards[owner].Count;
			graveyardCards[owner].AddRange(graveCards);
			return graveyardCards[owner].Count - oldCount;

		}

		public void SetCardDeckCount(int count, Owner owner)
		{
			if (!deckCardsCount.ContainsKey(owner))
			{
				RRLogger.Error("Error receiving cards in the deck");
				return;
			}

			deckCardsCount[owner] = count;
		}

		public int GetDeckCardCount(Owner owner)
		{
			if (!deckCardsCount.ContainsKey(owner))
			{
				RRLogger.Error("Error receiving cards in the deck");
				return 0;
			}

			return deckCardsCount[owner];
		}
		
		public int GetHandCardsCount(Owner owner)
		{
			return handCards.ContainsKey(owner)
				? handCards[owner].Count
				: 0;
		}
		
		public void ClearGraveyard(Owner owner)
		{
			if (graveyardCards.ContainsKey(owner))
				graveyardCards[owner].Clear();
		}

		public void ClearHand(Owner owner)
		{
			if (handCards.ContainsKey(owner))
				handCards[owner].Clear();
		}

		public void ClearTable(Owner owner)
		{
			if (tableCards.ContainsKey(owner))
				tableCards[owner].Clear();
		}

		public void RemoveDestroyedHandCards(Owner owner)
		{
			if (handCards.ContainsKey(owner))
				handCards[owner].RemoveAll(c => c.IsDestroyed);
		}

		public void AddTableCard(CreepCardEntity creepCardEntity)
		{
			if (creepCardEntity == null
			    || !tableCards.ContainsKey(creepCardEntity.Owner))
				return;

			tableCards[creepCardEntity.Owner].Add(creepCardEntity);
			OnTableChanged?.Invoke(creepCardEntity.Owner);
		}

		public void RemoveTableCard(TableCardEntity creepCardEntity)
		{
			if (creepCardEntity == null
			    || !tableCards.ContainsKey(creepCardEntity.Owner))
				return;

			if (!tableCards[creepCardEntity.Owner].Remove(creepCardEntity))
			{
				RRLogger.Error($"Cant remove card from {nameof(GameBus.LocalContext.tableCards)} by id: {creepCardEntity.Data.Id}");
				return;
			}

			OnTableChanged?.Invoke(creepCardEntity.Owner);
		}

		public void RemoveHandCard(HandCardEntity cardEntity)
		{
			if (cardEntity == null
			    || !handCards.ContainsKey(cardEntity.Owner))
				return;

			if (!handCards[cardEntity.Owner].Remove(cardEntity))
				RRLogger.Error($"[{"HandResolver".Orange().Bold()}] Couldn't remove hand card entity from collection {cardEntity.Owner}");
		}

		public void SetTurnTimerEnd(DateTime date)
		{
			if (date > TurnTimerEndsOn)
				TurnTimerEndsOn = date;
		}

		public void Clear()
		{
			GameEnded = false;
			OnTableChanged = null;
			TurnTimerEndsOn = DateTime.MinValue;
			RoundNumber = 0;
			tableCards.Clear();
			vulcanites.Clear();
			handCards.Clear();
			deckCardsCount.Clear();
			graveyardCards.Clear();
		}

		public override string ToString()
		{
			var stringBuilder = new StringBuilder($"{"CONTEXT!!!".Red()}\nRound = {RoundNumber} \n");

			stringBuilder.Append(
				$"{"Actors:".Olive()}\n{string.Join(" ; ", Vulcanites.Select(x => $"{x.Data.UserName} -> {x.Data.Id.ToString().Green()}").ToArray())}\n");
			stringBuilder.Append($"Table Self - {tableCards[Owner.Self].Count.ToString().Lightblue()} cards:\n".Olive());
			stringBuilder.Append(
				$"{string.Join("\n", tableCards[Owner.Self].Select(x => $"{x.Data.Title}, {x.Data.Id} -> {x.Data.UID.ToString().Green()}").ToArray())}\n");
			stringBuilder.Append($"Hand Self - {handCards[Owner.Self].Count.ToString().Lightblue()} cards:\n".Olive());
			stringBuilder.Append(
				$"{string.Join("\n", handCards[Owner.Self].Select(x => $"{x.Data.Title}, {x.Data.Id} -> {x.Data.UID.ToString().Green()}").ToArray())}\n");
			stringBuilder.Append($"Grave Self - {graveyardCards[Owner.Self].Count.ToString().Lightblue()} cards:\n".Olive());
			stringBuilder.Append(
				$"{string.Join("\n", graveyardCards[Owner.Self].Select(x => $"{x.Title}, {x.Id} -> {x.UID.ToString().Green()}").ToArray())}\n");
			stringBuilder.Append($"Deck Self - {deckCardsCount[Owner.Self].ToString().Lightblue()} cards:\n".Olive());
			stringBuilder.Append("\n");
			stringBuilder.Append($"Table Opponent - {tableCards[Owner.Opponent].Count.ToString().Lightblue()} cards:\n".Olive());
			stringBuilder.Append(
				$"{string.Join("\n", tableCards[Owner.Opponent].Select(x => $"{x.Data.Title}, {x.Data.Id} -> {x.Data.UID.ToString().Green()}").ToArray())}\n");
			stringBuilder.Append($"Hand Opponent- {handCards[Owner.Opponent].Count.ToString().Lightblue()} cards:\n".Olive());
			stringBuilder.Append(
				$"{string.Join("\n", handCards[Owner.Opponent].Select(x => $"{x.Data.Title}, {x.Data.Id} -> {x.Data.UID.ToString().Green()}").ToArray())}\n");
			stringBuilder.Append($"Grave Opponent - {graveyardCards[Owner.Opponent].Count.ToString().Lightblue()} cards:\n".Olive());
			stringBuilder.Append(
				$"{string.Join("\n", graveyardCards[Owner.Opponent].Select(x => $"{x.Title}, {x.Id} -> {x.UID.ToString().Green()}").ToArray())}\n");
			stringBuilder.Append($"Deck Opponent - {deckCardsCount[Owner.Opponent].ToString().Lightblue()} cards:\n".Olive());

			return stringBuilder.ToString();
		}
	}
}