using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.Data.Enums
{
	[JsonConverter(typeof(StringEnumConverter)), Flags]
	public enum CmdExecutionLevel
	{
		BeforeSessionStared = 2,
		AfterSessionEnded = 4,
		/// <summary>
		/// Allowed to execute between game rounds
		/// </summary>
		WrapAllowed = 8,
	}
}