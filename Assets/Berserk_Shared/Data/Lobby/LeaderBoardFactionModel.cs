using Berserk.Shared.Data.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Lobby
{
	public class LeaderBoardFactionModel
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public Faction Faction { get; set; }
		public int Win { get; set; }
		public int Lose { get; set; }
	}
}