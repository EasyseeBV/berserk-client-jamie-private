namespace RR.Game.TutorialSystemV2.Abstraction
{
	public interface ITutorialIdentity
	{
		/// <summary>
		/// To identify selections at execution.
		/// Can be null, Type.Name or Object.name will be used instead of identifiers.
		/// </summary>
		string[] Ids { get; }
	}
}