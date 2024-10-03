using Berserk.Shared.Data.Abstraction;
using Newtonsoft.Json;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public class ChangeCardPosition : LogicEvent
	{
		public int Id { get; }
		public int Position { get; }

		[JsonConstructor]
		public ChangeCardPosition(int id, int position)
		{
			Id = id;
			Position = position;
		}

		public ChangeCardPosition(IRuntimeCardData data)
		{
			Id = data.Id;
			Position = data.RelativePositionX;
		}
	}
}