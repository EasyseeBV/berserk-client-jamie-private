using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Campaign
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum CampaignEncounterType
	{
		Regular = 0,
		Elite,
		EpicBoss
	}
}
