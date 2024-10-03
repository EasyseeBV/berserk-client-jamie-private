using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Berserk.Shared.Data.Game;
using Berserk.Shared.Data.Identity;
using Berserk.Shared.Data.Lobby;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.DataBase;
using BerserkV3.Common.SerializedHelper;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using RR.Game.TutorialSystemV2.Realizations;

namespace BerserkV3.Startup.Authorization
{
	public static class User
	{
		public static bool IsAuthorized => !string.IsNullOrEmpty(AccessToken)
		                                   && !string.IsNullOrEmpty(RefreshToken)
		                                   && !string.IsNullOrEmpty(Id);
		public static UserDataModel Data { get; private set; }
		public static string Id => Data.Id;
		public static string UserName => Data.UserName;
		public static string AccessToken => Data.AccessToken;
		public static string RefreshToken => Data.RefreshToken;
		public static bool IsAnonymous => Data.IsAnonymous;
		public static List<OwnedCard> OwnedCards => Data.OwnedCards;
		public static List<DeckData> Decks => Data.Decks;
		public static DeckData ActiveDeck 
		{
			get => Decks.FirstOrDefault(x => x.Id == Data?.LastDeckId); 
			set => Data.LastDeckId = value?.Id;
		}
		public static List<OwnedVulcanite> OwnedVulcanites => Data.OwnedVulcanites;
		public static DateTime? LastLogin => Data.LastLogin;
		private static List<object> RedirectionArgs { get; } = new();
		
		static User()
		{
			Reset();
		}
		
		public static void Sync(UserDataModel data)
		{
			Data = data ?? throw new NullReferenceException($"{nameof(UserDataModel)} is missing");
			ValidateUserData();
		}

		public static void Reset()
		{
			Data = new UserDataModel();
		}

		public static void LogOut()
		{
			Reset();
			SerializeHelperAdapter.Service.Delete(SerializeKeyHelper.REMEMBER_CREEDS);
			SerializeHelperAdapter.Service.Delete(SerializeKeyHelper.EMAIL);
			SerializeHelperAdapter.Service.Delete(SerializeKeyHelper.ACCESS_TOKEN);
			SerializeHelperAdapter.Service.Delete(SerializeKeyHelper.REFRESH_TOKEN);
			SerializeHelperAdapter.Service.Delete(SerializeKeyHelper.USERID);
			SerializeHelperAdapter.Service.Save();
		}
		
		public static async UniTask LogOutAsync()
		{
			LogOut();
			await TutorialAdapter.Application.ResetAsync();
			await UniTask.Delay(TimeSpan.FromSeconds(0.5f)); // extra wait
		}

		public static string ValidateUserData(bool logger = true)
		{
			var builder = new StringBuilder();
			builder.AppendLine($"[{nameof(ValidateUserData).Orange()}] {UserName}");
			try
			{
				builder.AppendLine($"=========CARDS=========");
				builder.AppendLine($"Total account cards : {MarkValid(OwnedCards.Count)}");
				builder.AppendLine($"Total owned cards : {MarkValid(OwnedCards.Count(x => x.IsOwned))}");
				builder.AppendLine($"Total nft cards : {MarkValid(OwnedCards.Count(x => x.IsNft))}");
				builder.AppendLine($"Total subscription cards : {MarkValid(OwnedCards.Count(x => x.IsSubscription))}");
				builder.AppendLine($"Total valid cards : {MarkValid(OwnedCards.Count(x => x.IsValid()))}");
				builder.AppendLine($"Total inValid cards : {MarkInValid(OwnedCards.Count(x => !x.IsValid()))}");
			}
			catch (Exception e)
			{
				builder.AppendLine(e.Message);
			}
			try
			{
				builder.AppendLine($"=========DECKS=========");
				builder.AppendLine($"Total decks : {Decks.Count}");
			}
			catch (Exception e)
			{
				builder.AppendLine(e.Message);
			}
			
			foreach (var deck in Decks)
			{

				try
				{
					var deckOwnedCards = OwnedCards.Where(x => deck.OwnedCardIds.Contains(x.Id)).ToList();
					builder.AppendLine($"========={deck.Name}=========");
					builder.AppendLine($"Total cards : {MarkValid(deckOwnedCards.Count)}");
					builder.AppendLine($"Total owned cards : {MarkValid(deckOwnedCards.Count(x => x.IsOwned))}");
					builder.AppendLine($"Total nft cards : {MarkValid(deckOwnedCards.Count(x => x.IsNft))}");
					builder.AppendLine($"Total subscription cards : {MarkValid(deckOwnedCards.Count(x => x.IsSubscription))}");
					builder.AppendLine($"Total valid cards : {MarkValid(deckOwnedCards.Count(x => x.IsValid()))}");
					builder.AppendLine($"Total inValid cards : {MarkInValid(deckOwnedCards.Count(x => !x.IsValid()))}");
					builder.AppendLine($"Is current deck : {MarkValidBool(ActiveDeck?.Id == deck.Id)}");
				}
				catch (Exception e)
				{
					builder.AppendLine(e.Message);
				}
				
				try
				{
					var vulc = OwnedVulcanites.FirstOrDefault(x => x.Id == deck.OwnedVulcaniteId);
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
				builder.AppendLine($"Total vulcanites : {MarkValid(OwnedVulcanites.Count)}");
				builder.AppendLine($"Total owned vulcanites : {MarkValid(OwnedVulcanites.Count(x => x.IsOwned))}");
				builder.AppendLine($"Total rented vulcanites : {MarkValid(OwnedVulcanites.Count(x => x.IsRent))}");
				builder.AppendLine($"Total expired rent vulcanites : {MarkInValid(OwnedVulcanites.Count(x => x.IsExpireRent))}");
				builder.AppendLine($"Total valid vulcanites : {MarkValid(OwnedVulcanites.Count(x => x.IsValid()))}");
				builder.AppendLine($"Total inValid vulcanites : {MarkInValid(OwnedVulcanites.Count(x => !x.IsValid()))}");
			}
			catch (Exception e)
			{
				builder.AppendLine(e.Message);
			}
			
			foreach (var vulcanite in OwnedVulcanites)
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

		#region Redirection

		public static void AddRedirection(params object[] values)
		{
			RedirectionArgs.AddRange(values.Where(x=> x != null));
		}
		
		public static bool HasRedirection<T>()
		{
			return RedirectionArgs.OfType<T>().Any();
		}

		public static T[] GetRedirections<T>()
		{
			return RedirectionArgs.OfType<T>().ToArray();
		}

		public static void RemoveRedirections<T>(params object[] values)
		{
			RedirectionArgs.RemoveAll(x=> x is null or T || values.Contains(x));
		}

		#endregion
	}
}