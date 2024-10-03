using Newtonsoft.Json;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public class CommandApprove : LogicEvent
	{
		public string CommandId { get; }
		
		[JsonConstructor]
		public CommandApprove(string commandId)
		{
			CommandId = commandId;
		}
	}
}