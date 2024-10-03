using System.Collections.Generic;

namespace Berserk.Shared.Data.Game
{
	public class RuntimeAiData : RuntimePlayerData
	{
		public List<RuntimeAiArg> CommandBuffer { get; set; } = new();
	}
}