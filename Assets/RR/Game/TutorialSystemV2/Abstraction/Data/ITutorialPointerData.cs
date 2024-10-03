using RR.Game.TutorialSystemV2.Data;

namespace RR.Game.TutorialSystemV2.Abstraction.Data
{
	public interface ITutorialPointerData
	{
		TutorialVector3 StartPosition { get; }

		TutorialVector3 EndPosition { get; }
	}
}