using System.Collections.Generic;
using Berserk.Shared.GameCore.Abstraction;

namespace BerserkV3.GameCore.LogicEventsProcessor.BatchSystem
{
	public interface IBatcher
	{
		bool ShouldPerformBatching(IList<ILogicEvent> mainQueue);

		void Process(IList<ILogicEvent> mainQueue);
	}
}