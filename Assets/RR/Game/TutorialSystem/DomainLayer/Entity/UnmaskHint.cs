namespace RR.Game.TutorialSystem.Domain
{
	public class UnmaskHint
	{
		public HintVector HintVector { get; }
		public bool IsUnmask { get; }
		public string Meta { get; }

		public UnmaskHint(string meta, HintVector hintVector, bool isUnmask)
		{
			Meta = meta;
			HintVector = hintVector;
			IsUnmask = isUnmask;
		}
	}
}