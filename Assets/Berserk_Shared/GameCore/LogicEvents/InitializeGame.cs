using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Game;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public class InitializeGame : LogicEvent
	{
		public IRuntimePlayerData[] RuntimePlayerDatas;
		public RuntimeGeneratorsData RuntimeGeneratorsData;
		public IRuntimeContextData RuntimeContextData;
		public IRuntimeTimerData RuntimeTimerData;
		public IRuntimeData[] GameRuntimeDatas;
		public bool ReInitialize;
	}
}