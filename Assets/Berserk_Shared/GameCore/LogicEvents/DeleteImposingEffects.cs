namespace Berserk.Shared.GameCore.LogicEvents
{
	public class DeleteImposingEffects : LogicEvent
	{
		public string[] ImposingEffectIds { get; }
		public int RuntimeObjectId { get; }

		public DeleteImposingEffects(int runtimeObjectId, params string[] imposingEffectIds)
		{
			ImposingEffectIds = imposingEffectIds;
			RuntimeObjectId = runtimeObjectId;
		}
	}
}