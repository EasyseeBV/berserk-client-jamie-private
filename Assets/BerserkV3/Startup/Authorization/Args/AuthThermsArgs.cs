using BerserkV3.Common.LiveLinkRouter;

namespace BerserkV3.Startup.Authorization
{
	public struct AuthThermsArgs : IAuthArg
	{
		public AuthButtonArg Accept;
		public AuthButtonArg Footer;

		public static AuthThermsArgs Default(string stateId = null, AuthButtonArg? footer = null)
		{
			return new AuthThermsArgs
			{
				Accept = new AuthButtonArg
				{
					State = stateId,
					Text = "I Agree",
				},
				Footer = footer ?? new AuthButtonArg
				{
					Text = "I have read and agree to the <color=#F55D0D>Terms</color> and <color=#F55D0D>Privacy Policy</color>",
					Url = LinkKeyHelper.THERMS,
					UrlAsId = true
				}
			};
		}
	}
}