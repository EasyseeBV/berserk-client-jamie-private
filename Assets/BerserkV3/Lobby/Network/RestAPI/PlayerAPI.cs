using System.Collections.Generic;
using System.Threading.Tasks;
using Berserk.Shared.Data.Game;
using Berserk.Shared.Data.Identity;
using Berserk.Shared.Data.Lobby.Statistics;
using Berserk.Shared.Data.UserInventory;
using BerserkV3.Common.Network;
using BerserkV3.Startup.Authorization;
using RR.Core.DebugSystem;
using RR.Network.Rest;

namespace BerserkV3.Lobby.Network
{
	public class PlayerAPI : API<PlayerAPI>
	{
		public override string BaseUrl => URLs.APIUrl;
		public override string AuthToken => User.AccessToken;
		
		protected override void OnInit()
		{
			base.OnInit();
			OnRequest += WriteLogSecure;
			OnResponse += WriteLogSecure;
		}
		private void WriteLogSecure(string message)
		{
			RRLogger.Warning(message);
		}
		
		public static async Task<APIResponse<OwnedVulcanite>> PatchSelectVulcanite(SelectHeroDto data)
		{
			return await PatchAsync<OwnedVulcanite>("Player/SelectVulcanite", data);
		}
		
		public static async Task<APIResponse<UserStatisticsData>> GetStatistics()
		{
			return await GetAsync<UserStatisticsData>("Player/Statistics");
		}
		
		public static async Task<APIResponse<List<UserStatisticsLeagueData>>> GetStatisticsLeague()
		{
			return await GetAsync<List<UserStatisticsLeagueData>>("Player/StatisticsLeague");
		}
		
		public static async Task<APIResponse<List<UserCardStatisticsData>>> GetCardStatistics()
		{
			return await GetAsync<List<UserCardStatisticsData>>("Player/CardStatistics");
		}
		
		public static async Task<APIResponse<List<UserSessionData>>> GetStatisticsSessions()
		{
			return await GetAsync<List<UserSessionData>>("Player/StatisticsSessions");
		}

		public static async Task<APIResponse<UserAnalytics>> GetAnalyticsAsync()
		{
			return await GetAsync<UserAnalytics>("Player/GetAnalytics");
		}
		
		public static async Task<APIResponse<string>> PostAnalytics(UserAnalytics data)
		{
			return await PostAsync<string>("Player/PostAnalytics", data);
		}
		
		public static async Task<APIResponse<UserInventoryModel>> GetUserInventory()
		{ 
			return await GetAsync<UserInventoryModel>("Player/UserInventoryFullModels");
		}
	}
}