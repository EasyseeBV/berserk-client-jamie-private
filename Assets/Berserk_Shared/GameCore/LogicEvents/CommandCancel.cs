using Newtonsoft.Json;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public class CommandCancel : LogicEvent
	{
		public string CommandId { get; }
		public string Message { get; }
		
		[JsonConstructor]
		public CommandCancel(string commandId, string message)
		{
			CommandId = commandId;
			Message = message;
		}
	}
}