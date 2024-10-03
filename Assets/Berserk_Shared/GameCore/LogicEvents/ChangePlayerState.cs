using Berserk.Shared.Data.Abstraction;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public class ChangePlayerState : LogicEvent
	{
		public IRuntimePlayerData RuntimePlayerData { get; }

		public ChangePlayerState(IRuntimePlayerData runtimePlayerData)
		{
			RuntimePlayerData = runtimePlayerData.Clone(); // capture the state
		}
	}
}