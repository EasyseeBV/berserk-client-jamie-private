using System.Threading.Tasks;
using Berserk.Shared.Data.Campaign;
using Berserk.Shared.Data.Lobby;
using BerserkV3.Common.Network;
using BerserkV3.Startup.Authorization;
using RR.Core.DebugSystem;
using RR.Network.Rest;

namespace BerserkV3.Lobby.Network
{
	public class CampaignAPI : API<CampaignAPI>
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

		public static async Task<APIResponse<CampaignProgressModel>> GetProgress()
		{
			return await GetAsync<CampaignProgressModel>("Campaign/Progress");
		}

		public static async Task<APIResponse<ActiveSessionModel>> PostStartStage(CampaignStartStageModel model)
		{
			return await PostAsync<ActiveSessionModel>("Campaign/StartStage", model);
		}

		public static async Task<APIResponse<CampaignProgressModel>> PostChooseTreasure(CampaignChooseTreasureModel model)
		{
			return await PostAsync<CampaignProgressModel>("Campaign/ChooseTreasure", model);
		}

		public static async Task<APIResponse<CampaignProgressModel>> PostClaimMilestone(CampaignClaimMilestoneModel model)
		{
			return await PostAsync<CampaignProgressModel>("Campaign/ClaimMilestone", model);
		}
	}
}
