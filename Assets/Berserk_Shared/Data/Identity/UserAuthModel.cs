using System;

namespace Berserk.Shared.Data.Identity
{
	public class UserAuthModel
	{
		public string Id { get; set; }
		public string AccessToken { get; set; }
		public string RefreshToken { get; set; }
		public string UserName { get; set; }
		public string Email { get; set; }
        public DateTime? PreviousLogin { get; set; }
        public bool IsAcceptedPrivacyPolicy { get; set; }
	}
}
