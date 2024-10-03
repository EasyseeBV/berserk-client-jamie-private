namespace RR.Game.TutorialSystemV2.Abstraction
{
	public interface ITutorialOrderable
	{
		/// <summary>
		/// The order of execution or operation on an array.
		/// </summary>
		int Order { get; }
	}
}