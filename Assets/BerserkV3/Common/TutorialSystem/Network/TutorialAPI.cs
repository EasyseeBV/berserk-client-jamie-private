using System.Threading.Tasks;
using Berserk.Shared.Data.Tutor;
using BerserkV3.Common.Network;
using BerserkV3.Startup.Authorization;
using RR.Network.Rest;

namespace BerserkV3.Common.TutorialSystem
{
	public class TutorialAPI : API<TutorialAPI>
	{
		public override string BaseUrl => URLs.APIUrl;
		
		public override string AuthToken => User.AccessToken;

		public static async Task<APIResponse<TutorProgressModel>> GetProgress()
		{
			return await GetAsync<TutorProgressModel>("Tutorial/GetProgress");
		}

		public static async Task<APIResponse<string>> PostProgress(TutorProgressModel model)
		{
			return await PostAsync<string>("Tutorial/PostProgress", model);
		}

		public static async Task<APIResponse<string>> GetConfig()
		{
			return await GetAsync<string>("Tutorial/GetConfig");
		}
	}
}