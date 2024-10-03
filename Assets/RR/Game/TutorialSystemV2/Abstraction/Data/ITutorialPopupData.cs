using RR.Game.TutorialSystemV2.Data;

namespace RR.Game.TutorialSystemV2.Abstraction.Data
{
	public interface ITutorialPopupData
	{
		string Title { get; }

		string Text { get; }

		string ContinueButtonText { get; }

		TutorialVector2 Position { get; }

		TutorialVector2 SizeDelta { get; }

		float DelayBeforShow { get; }

		bool ArrowRequired { get; }

		bool ContinueButton { get; }

		bool BackButton { get; }

		bool BackHinder { get; }

		bool AutoSize { get; }

		bool Enabled { get; }
	}
}