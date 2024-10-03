using System;

namespace BerserkV3.Startup.Authorization
{
	public struct AuthButtonArg : IAuthArg
	{
		public string Text;
		public string State;
		public string Url;
		public bool UrlAsId;
		public Action CallBack;
	}
}