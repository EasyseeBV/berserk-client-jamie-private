using System.Collections.Generic;
using Berserk.Shared.GameCore.Abstraction;

namespace BerserkV3.GameCore.LogicEventsProcessor.BatchSystem
{
	public interface ILogicEventsBatcher
	{
		void Process(List<ILogicEvent> mainQueue);
		IEnumerable<ILogicEvent> UnpackBatch(ILogicEvent logicEvent);
	}
}