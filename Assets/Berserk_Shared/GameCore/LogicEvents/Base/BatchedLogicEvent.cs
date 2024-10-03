using System.Collections.Generic;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public class BatchedLogicEvent<T> : LogicEvent where T : ILogicEvent
	{
		public List<T> Events { get; set; } = new();
	}
}
