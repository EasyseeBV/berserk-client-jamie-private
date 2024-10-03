using Newtonsoft.Json;

namespace Berserk.Shared.GameCore.LogicEvents
{

	public class BatchEvent : LogicEvent
	{
		public int Id { get; }
		public bool Start { get; }
		public bool End { get; }

		[JsonConstructor]
		public BatchEvent(int id, bool start, bool end)
		{
			Id = id;
			Start = start;
			End = end;
		}

		public BatchEvent(int id, bool isStart)
		{
			Id = id;
			Start = isStart;
			End = !Start;
		}
	}
}