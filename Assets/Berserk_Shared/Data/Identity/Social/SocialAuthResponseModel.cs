using Newtonsoft.Json;

namespace Berserk.Shared.Data.Identity.Social
{
	public class SocialAuthResponseModel
	{
		[JsonProperty("access_token")]
		public string AccessToken { get; set; }
	}
}