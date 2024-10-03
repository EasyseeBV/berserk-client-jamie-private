using BerserkV3.Startup.Authorization;

namespace BerserkV3.Startup.UI
{
	public struct AuthResetPassArgs : IAuthArg
	{
		public string Email { get; set; }
	}
}