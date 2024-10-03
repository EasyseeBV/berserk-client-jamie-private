using Newtonsoft.Json;

namespace Berserk.Shared.GameCore.Models
{
	public class RuntimeEffectModel
	{
		public string DataId { get; }
		public int[] TargetIds { get; }
		public int ExecutorId { get; }
		public int CurrentLength { get; }
		public int CurrentValue { get; }

		[JsonConstructor]
		public RuntimeEffectModel(string dataId, int[] targetIds, int executorId, int currentLength,
			int currentValue)
		{
			DataId = dataId;
			TargetIds = targetIds;
			ExecutorId = executorId;
			CurrentLength = currentLength;
			CurrentValue = currentValue;
		}
	}
}