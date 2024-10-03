using Berserk.Shared.Data.Enums;
using Newtonsoft.Json;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public class InvalidActionEvent : LogicEvent
	{
		public InvalidAction Value { get; }
		public string Id { get; }

		[JsonConstructor]
		public InvalidActionEvent(InvalidAction value, string id)
		{
			Value = value;
			Id = id;
		}
		
		public InvalidActionEvent(InvalidAction value)
		{
			Value = value;
			Id = null;
		}
		
		public InvalidActionEvent(string id)
		{
			Value = InvalidAction.None;
			Id = id;
		}
	}
}