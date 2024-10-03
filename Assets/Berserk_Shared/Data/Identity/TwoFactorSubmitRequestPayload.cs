using System;

namespace Berserk.Shared.Data.Identity
{
	[Obsolete("Client not used this model")]
	public class TwoFactorSubmitRequestPayload
	{
		public string Email { get; set; }
		public string Secret { get; set; }
		public string Code { get; set; }
	}
}