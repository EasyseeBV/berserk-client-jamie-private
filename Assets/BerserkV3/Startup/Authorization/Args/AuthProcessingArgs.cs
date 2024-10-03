using Berserk.Shared.Data.Identity;

namespace BerserkV3.Startup.Authorization
{
	public struct AuthProcessingArgs : IAuthArg
	{
		public UserAuthModel AuthModel;
	}
}