using RR.Game.TutorialSystemV2.Data;

namespace RR.Game.TutorialSystemV2.Abstraction.Data
{
	public interface ITutorialUnmaskData
	{
		TutorialVector2 Position { get; }

		TutorialVector2 Size { get; }

		string Meta { get; }

		bool HideIfMissTarget { get; }
		
		bool DisableReposition { get; }
		
		bool FitTargetPosition { get; }
		
		bool DisableResize { get; }
		
		bool FitTargetSize { get; }
		
		bool AutoRefresh { get; }
	}
}