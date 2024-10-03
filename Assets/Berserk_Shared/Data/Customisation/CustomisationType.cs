using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Customisation
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum CustomisationType
	{
		None,
		AvatarFrame,
		Gameboard,
		Background,
		CardBack,
		Emotions,
		LobbyMusic,
		BattleMusic
	}
}