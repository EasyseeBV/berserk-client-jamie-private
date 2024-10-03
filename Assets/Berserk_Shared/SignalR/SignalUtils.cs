using Newtonsoft.Json;

namespace Berserk.Shared.SignalR
{
	public static class SignalUtils
	{
		public static readonly JsonSerializerSettings SOCKET_SIGNAL_SETTINGS = new()
		{
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
			NullValueHandling = NullValueHandling.Ignore,
			MaxDepth = 4,
			PreserveReferencesHandling = PreserveReferencesHandling.Objects,
		};
	}
}