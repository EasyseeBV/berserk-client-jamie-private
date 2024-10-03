using Newtonsoft.Json;

namespace Berserk.Shared.GameCore.EffectSystem
{
	public class EffectInfo
	{
		public string OwnerUserId { get; }
		public int ExecutorId { get; }
		public int[] TargetIds { get; }
		public string EffectId { get; }

		//required for effects that can apply other effects
		public string FromEffectId { get; }

		[JsonConstructor]
		public EffectInfo(string ownerUserId, int executorId, int[] targetIds, string effectId, string fromEffectId = "")
		{
			OwnerUserId = ownerUserId;
			ExecutorId = executorId;
			TargetIds = targetIds;
			EffectId = effectId;
			FromEffectId = !string.IsNullOrEmpty(fromEffectId)
				? fromEffectId
				: effectId;
		}

		public override string ToString()
		{
			return $"{ExecutorId} -> {TargetIds}: ({EffectId})";
		}
	}
}