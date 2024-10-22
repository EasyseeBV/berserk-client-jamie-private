using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Berserk.Shared.Data.Identity;
using Berserk.Shared.Data.UserInventory;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.DataBase;
using BerserkV3.Common.SerializedHelper;
using BerserkV3.Startup.Authorization.Inventory.Models;
using BerserkV3.Startup.Authorization.Inventory.Realizations;
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
		public static DateTime? LastLogin => Data.LastLogin;
		private static List<object> RedirectionArgs { get; } = new();
		
		static User()
		{
			Reset();
		}
		
		public static void Sync(UserDataModel data)
		{
			Data = data ?? throw new NullReferenceException($"{nameof(UserDataModel)} is missing");
			
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