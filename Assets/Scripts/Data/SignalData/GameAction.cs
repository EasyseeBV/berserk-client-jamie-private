using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Vulcan.Data
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum GameAction
	{
		None = 0,
		PlayedCard = 1,
		RoundChanged = 4,
		RoundAccepted = 6,
		ModifyEntity = 8,
		BotTakeControl = 16,
		UserConnected = 32,
		TimerPaused = 41,
		TimerResumed = 42,
		SessionEnd = 64,
		HostChanged = 128,
		AddedHandCard = 256,
		PlayerKicked = 1024,
		PlayerResigned = 2048,
		TimerTick = 4096,
		MulliganPlayerReady = 8192,
		MulliganFinished = 9000,
		PerformAction = 16384,
		PlayerBanned = 32768,
		Commended = 65536,
		PlayerReaction = 131072,
		Surrender = 262144
	}
}