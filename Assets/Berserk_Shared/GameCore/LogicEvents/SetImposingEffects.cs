namespace Berserk.Shared.GameCore.LogicEvents
{

	public class SetImposingEffects : LogicEvent
	{
		public string[] ImposingEffectIds { get; }
		public int RuntimeObjectId { get; }

		public SetImposingEffects(int runtimeObjectId, params string[] imposingEffectIds)
		{
			ImposingEffectIds = imposingEffectIds;
			RuntimeObjectId = runtimeObjectId;
		}
	}

}