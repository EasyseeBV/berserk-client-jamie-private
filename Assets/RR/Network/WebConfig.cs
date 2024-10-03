using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace RR.Network
{
	public static class WebConfig
    {
        public const string ContentType = "application/json; charset=UTF-8";

        public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
#if DEBUG
            Formatting = Formatting.Indented,
#endif
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateParseHandling = DateParseHandling.DateTime,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public static readonly JsonSerializer JsonSerializer = new JsonSerializer
        {
#if DEBUG
            Formatting = Formatting.Indented,
#endif
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateParseHandling = DateParseHandling.DateTime,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
    }
}
