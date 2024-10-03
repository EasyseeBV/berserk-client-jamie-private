using System.Collections.Generic;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Commands;
using Berserk.Shared.GameCore.Models;

namespace BerserkV3.GameCore.Prediction.Abstraction
{
	public interface IPredictProcessor
	{
		void PerformCmd<T>(CmdParamsModel model) where T : Command;
		void ProcessPredictedQueue(IList<ILogicEvent> events);
	}
}