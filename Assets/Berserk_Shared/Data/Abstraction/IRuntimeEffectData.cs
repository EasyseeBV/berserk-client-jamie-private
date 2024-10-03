using System.Collections.Generic;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.Data.Abstraction
{
	public interface IRuntimeEffectData : IRuntimeDataBase
	{
		int Id { get; set; }
		int ExecutorId { get; set; }
		string ExecutorOwnerId { get; set; }
		string ConfigId { get; set; }
		int CurrentLength { get; set; }
		int CurrentValue { get; set; }
		int DisabledLength { get; set; }
		List<int> TargetIds { get; set; }
		List<int> AppliedIds { get; set; }
		List<IEffectRuntimeArg> RuntimeArgs { get; set; }
		AccessLevel AccessLevel { get; set; }
		bool IsInnate { get; set; }
		TArg GetRuntimeArg<TArg>();
		IEnumerable<TArg> GetRuntimeArgs<TArg>();
		bool Disabled => DisabledLength is -1 or > 0;
	}
}