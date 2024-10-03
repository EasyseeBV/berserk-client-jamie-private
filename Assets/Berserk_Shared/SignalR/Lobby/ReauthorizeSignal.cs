using Berserk.Shared.SignalR.Abstractions;
using Berserk.Shared.SignalR.Enums;
using Newtonsoft.Json;

namespace Berserk.Shared.SignalR.Lobby
{
	public class ReauthorizeSignal : ISocketSignal
	{
		public SignalType Type => SignalType.Reauthorization;
		public string Message { get; }

		public ReauthorizeSignal(string message)
		{
			Message = message;
		}

		public override string ToString() => JsonConvert.SerializeObject(this, SignalUtils.SOCKET_SIGNAL_SETTINGS);
	}
}