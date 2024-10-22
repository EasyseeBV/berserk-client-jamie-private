using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.UserInventory;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.DataBase;
using BerserkV3.Lobby.Network;
using BerserkV3.Startup.Authorization.Inventory.Models;
using RR.Core.Extensions;

namespace BerserkV3.Startup.Authorization.Inventory.Realizations
{
	public class InventoryApplication : IInventoryApplication
	{
		private readonly Dictionary<Type, IInventoryItemRepository> repositoryByType = new();
		
		public async Task<bool> InitAsync()
		{
			var response = await PlayerAPI.GetUserInventory();
			
			if (!response.IsSuccess || response.Data == null)
			{
				return false;
			}
			
			var itemList = new List<IInventoryItem>();
			
			var properties = response.Data.GetType().GetProperties();
			foreach (var property in properties)
			{
				if (typeof(IEnumerable<IInventoryItem>).IsAssignableFrom(property.PropertyType))
				{
					var items = property.GetValue(response.Data) as IEnumerable<IInventoryItem>;
					if (items != null)
					{
						itemList.AddRange(items);
					}
				}
			}

			if (itemList.Count == 0)
				return false;

			foreach (var itemsGroup in itemList.GroupBy(x => x.GetType()))
			{
				InitRepository(itemsGroup, itemsGroup.Key);
			}

			return true;
		}
		
		private void InitRepository(IEnumerable<IInventoryItem> items, Type type)
		{
			if (!repositoryByType.TryGetValue(type, out var repository))
				repositoryByType[type] = repository = new InventoryItemRepository();
			
			repository.Items.Clear();
			repository.Items.AddRange(items);
		}
		
		public T Get<T>(string id) where T : IInventoryItem
		{
			return repositoryByType.TryGetValue(typeof(T), out var repository)
				? repository.Items.OfType<T>().FirstOrDefault(x => x.Id == id)
				: default;
		}
		
		public T[] Get<T>() where T : IInventoryItem
		{
			return repositoryByType.TryGetValue(typeof(T), out var repository)
				? repository.Items.OfType<T>().ToArray()
				: default;
		}
		
		public void Remove(IInventoryItem item)
		{
			if (item == null || !repositoryByType.TryGetValue(item.GetType(), out var repository))
				return;

			repository.Items.RemoveAll(x => x.Id == item.Id);
		}

		public void Add(IInventoryItem item)
		{
			if (item == null || !repositoryByType.TryGetValue(item.GetType(), out var repository))
				return;

			repository.Items.Add(item);
		}
		
		public string ValidateUserData(bool logger = true)
		{
			var builder = new StringBuilder();
			builder.AppendLine($"[{nameof(ValidateUserData).Orange()}] {User.UserName}");
			var ownedCards = Get<OwnedCard>().ToList();
			try
			{
				builder.AppendLine($"=========CARDS=========");
				builder.AppendLine($"Total account cards : {MarkValid(ownedCards.Count)}");
				builder.AppendLine($"Total owned cards : {MarkValid(ownedCards.Count(x => x.IsOwned))}");
				builder.AppendLine($"Total nft cards : {MarkValid(ownedCards.Count(x => x.IsNft))}");
				builder.AppendLine($"Total subscription cards : {MarkValid(ownedCards.Count(x => x.IsSubscription))}");
				builder.AppendLine($"Total valid cards : {MarkValid(ownedCards.Count(x => x.IsValid()))}");
				builder.AppendLine($"Total inValid cards : {MarkInValid(ownedCards.Count(x => !x.IsValid()))}");
			}
			catch (Exception e)
			{
				builder.AppendLine(e.Message);
			}
			
			var decks = Get<OwnedDeck>().ToList();
			try
			{
				
				builder.AppendLine($"=========DECKS=========");
				builder.AppendLine($"Total decks : {decks.Count}");
			}
			catch (Exception e)
			{
				builder.AppendLine(e.Message);
			}
			
			var ownedVulcanites = Get<OwnedVulcanite>().ToList();
			foreach (var deck in decks)
			{

				try
				{
					var deckOwnedCards = ownedCards.Where(x => deck.OwnedCardIds.Contains(x.Id)).ToList();
					var activeDeck =  Get<OwnedDeck>().FirstOrDefault(x => x.Id == User.Data?.LastDeckId);
					builder.AppendLine($"========={deck.Name}=========");
					builder.AppendLine($"Total cards : {MarkValid(deckOwnedCards.Count)}");
					builder.AppendLine($"Total owned cards : {MarkValid(deckOwnedCards.Count(x => x.IsOwned))}");
					builder.AppendLine($"Total nft cards : {MarkValid(deckOwnedCards.Count(x => x.IsNft))}");
					builder.AppendLine($"Total subscription cards : {MarkValid(deckOwnedCards.Count(x => x.IsSubscription))}");
					builder.AppendLine($"Total valid cards : {MarkValid(deckOwnedCards.Count(x => x.IsValid()))}");
					builder.AppendLine($"Total inValid cards : {MarkInValid(deckOwnedCards.Count(x => !x.IsValid()))}");
					builder.AppendLine($"Is current deck : {MarkValidBool(activeDeck?.Id == deck.Id)}");
				}
				catch (Exception e)
				{
					builder.AppendLine(e.Message);
				}
				
				try
				{
					var vulc = ownedVulcanites.FirstOrDefault(x => x.Id == deck.OwnedVulcaniteId);
					builder.AppendLine($"========={deck.Name}/VULCANITE=========");
					builder.AppendLine($"Owned vulcanite IsValid : {MarkValidBool(vulc.IsValid())}");
					builder.AppendLine($"Owned vulcanite IsOwned: {MarkValidBool(vulc?.IsOwned)}");
					builder.AppendLine($"Owned vulcanite IsRent : {MarkValidBool(vulc?.IsRent)}");
					builder.AppendLine($"Owned vulcanite IsExpireRent : {MarkValidBool(vulc?.IsExpireRent)}");
				}
				catch (Exception e)
				{
					builder.AppendLine(e.Message);
				}
			}
			
			try
			{
				builder.AppendLine($"=========VULCANITES=========");
				builder.AppendLine($"Total vulcanites : {MarkValid(ownedVulcanites.Count)}");
				builder.AppendLine($"Total owned vulcanites : {MarkValid(ownedVulcanites.Count(x => x.IsOwned))}");
				builder.AppendLine($"Total rented vulcanites : {MarkValid(ownedVulcanites.Count(x => x.IsRent))}");
				builder.AppendLine($"Total expired rent vulcanites : {MarkInValid(ownedVulcanites.Count(x => x.IsExpireRent))}");
				builder.AppendLine($"Total valid vulcanites : {MarkValid(ownedVulcanites.Count(x => x.IsValid()))}");
				builder.AppendLine($"Total inValid vulcanites : {MarkInValid(ownedVulcanites.Count(x => !x.IsValid()))}");
			}
			catch (Exception e)
			{
				builder.AppendLine(e.Message);
			}
			
			foreach (var vulcanite in ownedVulcanites)
			{
				try
				{
					var hero = GameDataBaseAdapter.Instance.GetHero(vulcanite?.VulcaniteId);
					builder.AppendLine($"========={hero?.Name}=========");
					builder.AppendLine($"Owned vulcanite IsValid : {MarkValidBool(vulcanite.IsValid())}");
					builder.AppendLine($"Owned vulcanite IsOwned: {MarkValidBool(vulcanite?.IsOwned)}");
					builder.AppendLine($"Owned vulcanite IsRent : {MarkValidBool(vulcanite?.IsRent)}");
					builder.AppendLine($"Owned vulcanite IsExpireRent : {MarkValidBool(vulcanite?.IsExpireRent)}");
				}
				catch (Exception e)
				{
					builder.AppendLine(e.Message);
				}
			}

			var log = builder.ToString();
			if (logger)
				DefaultSharedLogger.Log(log);
			
			builder.Clear();
			return log;
			string MarkValidBool(bool? value)
			{
				if (!logger)
					return value.HasValue && value.Value ? "true" : "false";
				
				return value.HasValue && value.Value ? "true".Green() : "false".Orange();
			}
			string MarkInValid(int? value)
			{
				if (!logger)
					return $"{value ?? 0}";
				
				return value is > 0 ? $"{value}".Red() : $"{value}".Orange();
			}
			string MarkValid(int? value)
			{
				if (!logger)
					return $"{value ?? 0}";
				return value is > 0 ? $"{value}".Green() : $"{value}".Orange();
			}
		}
	}
}