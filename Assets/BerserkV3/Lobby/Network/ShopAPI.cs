using System.Collections.Generic;
using System.Threading.Tasks;
using BerserkV3.Common.Network;
using BerserkV3.Startup.Authorization;
using RR.Core.DebugSystem;
using RR.Network.Rest;
using ServerCore.Infrastructure.Models;
using ServerCore.Infrastructure.Models.InternalVulcan;

namespace BerserkV3.Lobby.Network
{
	public class ShopAPI : API<ShopAPI>
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
		
		public static async Task<APIResponse<List<ShopVulcaniteModel>>> GetShopVulcanites()
		{
			return await GetAsync<List<ShopVulcaniteModel>>("Shop/GetVulcanites");
		}

		public static async Task<APIResponse<string>> BuyVulcanite(string vulcaniteId)
		{
			return await PutAsync<string>($"Shop/BuyVulcanite?vulcaniteId={vulcaniteId}", (object) null);
		}

		public static async Task<APIResponse<UserStatsModel>> GetPlayerBalance()
		{
			return await GetAsync<UserStatsModel>("Shop/GetPlayerBalance");
		}
	}
}