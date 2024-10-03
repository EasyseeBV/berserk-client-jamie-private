using Newtonsoft.Json;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public class CommandWait : LogicEvent
	{
		public string CommandId { get; }
		
		[JsonConstructor]
		public CommandWait(string commandId)
		{
			CommandId = commandId;
		}
	}
}