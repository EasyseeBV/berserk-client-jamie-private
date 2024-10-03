namespace RR.Game.TutorialSystemV2.Abstraction.Data
{
	public interface ITutorialBlockData
	{
		string NextId { get; }

		float DelayBeforeInvoke { get; }

		float DelayAfterClose { get; }

		bool PauseRequired { get; }

		bool IsSyncRequired { get; }

		string[] KeywordTriggers { get; }

		string[] RequiredCompletedHints { get; }
		
		string[] CompleteDependedHints { get; }
	}
}