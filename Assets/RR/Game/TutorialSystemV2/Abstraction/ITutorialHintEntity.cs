using RR.Game.TutorialSystemV2.Abstraction.Data;

namespace RR.Game.TutorialSystemV2.Abstraction
{
	public interface ITutorialHintEntity
	{
		string Id { get; }
		
		string[] TriggerIds { get; }
		
		ITutorialBlockData Data { get; }

		ITutorialPopupData Popup { get; }

		ITutorialPointerData Pointer { get; }

		ITutorialConditionData[] Conditions { get; }

		ITutorialUnmaskData[] Unmasks { get; }
	}
}