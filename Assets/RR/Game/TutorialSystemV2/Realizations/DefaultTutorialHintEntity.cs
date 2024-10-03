using System;
using System.Linq;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Data;
using RR.Game.TutorialSystemV2.Data;

namespace RR.Game.TutorialSystemV2.Realizations
{
	public class DefaultTutorialHintEntity : ITutorialHintEntity
	{
		public string Id { get; }
		public ITutorialBlockData Data { get; }
		public ITutorialPopupData Popup { get; }
		public ITutorialPointerData Pointer { get; }
		public ITutorialConditionData[] Conditions { get; }
		public ITutorialUnmaskData[] Unmasks { get; }
		public string[] TriggerIds { get; }

		public DefaultTutorialHintEntity(TutorialBlock hint)
		{
			Id = hint.Id;
			Data = hint.Data;
			Popup = hint.Popup;
			Pointer = hint.Pointer;
			Conditions = hint.Conditions?.OfType<ITutorialConditionData>().ToArray();
			Unmasks = hint.Unmasks?.OfType<ITutorialUnmaskData>().ToArray();
			TriggerIds = Data?.KeywordTriggers?.Append(Id)
				                      .Where(x => !string.IsNullOrEmpty(x))
				                      .ToArray() ?? Array.Empty<string>();
		}
		
		public override string ToString()
		{
			return $"Id : {Id}, NextId : {Data.NextId}";
		}
	}
}