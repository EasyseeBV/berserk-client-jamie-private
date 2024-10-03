using Berserk.Shared.Data.Identity;

namespace BerserkV3.Startup.Authorization.ExternalProviders
{
	public struct ExternalProviderResponse
	{
		public string Message { get; }
		public bool Successful { get; }
		public string Code { get; set; }
		public UserAuthModel Model { get; set; }

		public ExternalProviderResponse(string message, bool successful)
		{
			Message = message;
			Successful = successful;
			Model = default;
			Code = string.Empty;
		}
	}
}