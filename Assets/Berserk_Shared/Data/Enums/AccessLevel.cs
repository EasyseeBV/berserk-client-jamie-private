using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{
	[JsonConverter(typeof(StringEnumConverter)), Flags]
	public enum AccessLevel
	{
		NoOne = 2,
		Self = 4,
		All = 8,
	}
}