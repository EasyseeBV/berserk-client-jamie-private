using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.Data.Game
{
	public class RuntimeEffectData : IRuntimeEffectData
	{
		public int Id { get; set; }
		public int ExecutorId { get; set; }
		public string ExecutorOwnerId { get; set; }
		public string ConfigId { get; set; }
		public int CurrentLength { get; set; }
		public int CurrentValue { get; set; }
		public int DisabledLength { get; set; }
		public List<int> TargetIds { get; set; } = new();
		public List<int> AppliedIds { get; set; } = new();
		public List<IEffectRuntimeArg> RuntimeArgs { get; set; } = new();
		public AccessLevel AccessLevel { get; set; }
		public bool IsInnate { get; set; }
		public TArg GetRuntimeArg<TArg>() => RuntimeArgs.OfType<TArg>().FirstOrDefault();
		public IEnumerable<TArg> GetRuntimeArgs<TArg>() => RuntimeArgs.OfType<TArg>();
		public EffectExecutionOrder ExecutionOrder { get; set; }
	}
}