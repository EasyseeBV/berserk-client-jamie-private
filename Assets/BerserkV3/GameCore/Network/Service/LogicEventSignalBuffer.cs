using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.GameCore.Abstraction;
using BerserkV3.GameCore.LogicEventsProcessor;
using Sirenix.Utilities;

namespace BerserkV3.GameCore.Network
{
	public class LogicEventSignalBuffer : SignalBuffer<ILogicEvent>
	{
		private readonly IGameLogicEventsProcessor logicEventsProcessor;

		public LogicEventSignalBuffer(IGameLogicEventsProcessor logicEventsProcessor)
		{
			this.logicEventsProcessor = logicEventsProcessor;
		}

		protected override void OnSignalsPassed(IList<ILogicEvent> signals)
		{
			base.OnSignalsPassed(signals);
			if(signals.IsNullOrEmpty())
				return;
			logicEventsProcessor.Process(signals.ToArray());
		}
	}
}