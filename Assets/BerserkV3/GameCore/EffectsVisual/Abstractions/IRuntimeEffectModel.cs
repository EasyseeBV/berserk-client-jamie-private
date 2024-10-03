using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Game;
using BerserkV3.GameCore.UI;

namespace BerserkV3.GameCore.EffectsVisual.Abstractions
{
	public interface IRuntimeEffectModel
	{
		int Id { get; }
		int ExecutorId { get; }
		int CurrentLength { get; set; }
		int CurrentValue { get; set; }
		int DisabledLength { get; set; }
		List<int> AppliedIds { get; }
		EffectData Data { get; }
		List<IEffectRuntimeArg> RuntimeArgs { get; }
		IRuntimeObjectView[] Targets { get; set; }
		IRuntimeObjectView Executor { get; }

		TArg GetRuntimeArg<TArg>() => RuntimeArgs.OfType<TArg>().FirstOrDefault();
		IEnumerable<TArg> GetRuntimeArgs<TArg>() => RuntimeArgs.OfType<TArg>();
		bool Disabled => DisabledLength is <= -1 or > 0;
	}
}