using RR.Network.Rest;

namespace BerserkV3.Startup.Authorization.ExternalProviders
{
	public class ExternalProviderBase
	{
		protected const string AUTH_CANCELLED = "{0} auth cancelled.";
		protected const string RESPONSE_IS_EMPTY = "{0} auth failed. Response is empty. Please try again or make a report.";
		protected const string RESPONSE_ERROR = "{0} auth failed. {1}";
		
		private const string BANNED = "banned";
		
		protected bool IsBannedResponse<T>(APIResponse<T> response) where T : class
		{
			return !response 
			       && response.RawMessage != null 
			       && response.RawMessage.ToLower().Contains(BANNED);
		}
		
		protected bool IsTimeoutResponse<T>(APIResponse<T> response) where T : class
		{
			return response.Code == 0;
		}
	}
}