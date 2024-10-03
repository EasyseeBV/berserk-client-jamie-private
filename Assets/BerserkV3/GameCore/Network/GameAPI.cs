using System;
using System.Net;
using System.Threading.Tasks;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;
using Berserk.Shared.GameCore.Models.API;
using BerserkV3.Common.Network;
using BerserkV3.Startup.Authorization;
using RR.Core.DebugSystem;
using RR.Network.Rest;

namespace BerserkV3.GameCore.Network
{
	public class GameAPI : API<GameAPI>
	{
		public static event Action<int, string> OnFail;
		public static event Action<Exception> OnException;

		public override string BaseUrl => URLs.APIUrl;
		public override string AuthToken => User.AccessToken;

		protected override void OnInit()
		{
			base.OnInit();
			OnRequestFail += OnFail;
			OnError += OnException;
			OnRequest += WriteLogSecure;
			OnResponse += WriteLogSecure;
		}
		private void WriteLogSecure(string message)
		{
			RRLogger.Warning(message);
		}
		
		public static async Task<HttpStatusCode> PostReportOpponent(ReportReason reportReason)
		{
			return await PostAsync("Game/ReportOpponent", new ReportModel(reportReason));
		}
		
		public static async Task PostUserCommend(CommendModel model)
		{
			await PostAsync("Game/CommendUser", model);
		}

		public static async Task<APIResponse<UserCommendsModel>> PostGetUserCommends(CommendModel model)
		{
			return await PostAsync<UserCommendsModel>("Game/GetUserCommends", model);
		}

		public static async Task PostEmotionAsync(PlayEmotionModel model)
		{
			await PostAsync("Game/PlayEmotion", model);
		}

		public static async Task<APIResponse<string>> GetGameDataBase()
		{
			return await GetAsync<string>("Game/GameDatabaseModel");
		}

		public static async Task<APIResponse<string>> GetGameConfig()
		{
			return await GetAsync<string>("Game/GameConfig");
		}
	}
}