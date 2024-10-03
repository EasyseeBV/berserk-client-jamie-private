using Berserk.Shared.Data.Identity.Social;

namespace BerserkV3.Startup.Authorization
{
	public struct AuthSocialSignInArgs : IAuthArg
	{
		public string ReturnState;
		public ExternalProvider Provider;
	}
}