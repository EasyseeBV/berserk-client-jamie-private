using Newtonsoft.Json;

namespace Berserk.Shared.GameCore.EffectSystem.TargetSystem
{
	public class TargetInfo
	{
		public string OwnerUserId { get; }
		public string[] EffectIds { get; }
		public int ExecutorId { get; }

		[JsonConstructor]
		public TargetInfo(string ownerUserId, string[] effectIds, int executorId)
		{
			OwnerUserId = ownerUserId;
			EffectIds = effectIds;
			ExecutorId = executorId;
		}
	}
}