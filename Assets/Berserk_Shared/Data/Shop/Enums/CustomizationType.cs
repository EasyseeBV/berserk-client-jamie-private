using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Shop.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum CustomizationType
	{
		BoardSkin,
		CardSkin,
		AdventureSkin,
		CompanionSkin,
		CardBacks,
		LoadingBackgroundScreen,
		LoadingFrameScreen,
		ProfileBorder,
		ProfileAvatar,
		Emote
	}
}