namespace Berserk.Shared.GameCore.Abstraction
{
	public interface ILogicEvent
	{
		bool NotPredictable { get; }
		bool IsForceSync { get; }
		ILogicEvent Reverse();
		public int Order { get; set; }
	}
}