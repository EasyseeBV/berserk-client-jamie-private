namespace Berserk.Shared.GameCore.LogicEvents
{
	public class DeleteObject : LogicEvent
	{
		public readonly int RuntimeObjectId;

		public DeleteObject(int runtimeObjectId)
		{
			RuntimeObjectId = runtimeObjectId;
		}
	}
}