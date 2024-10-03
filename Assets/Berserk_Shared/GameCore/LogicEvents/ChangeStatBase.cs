namespace Berserk.Shared.GameCore.LogicEvents
{
	public abstract class ChangeStatBase : LogicEvent
	{
		public int RuntimeObjectId { get; }
		public IntStat Stat { get; }

		protected ChangeStatBase(IntStat stat, int runtimeObjectId)
		{
			RuntimeObjectId = runtimeObjectId;
			Stat = stat.Clone(); // capture the state
		}
	}
}