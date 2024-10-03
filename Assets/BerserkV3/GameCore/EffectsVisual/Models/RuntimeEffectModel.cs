using System;
using System.Collections.Generic;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Game;
using BerserkV3.GameCore.EffectsVisual.Abstractions;
using BerserkV3.GameCore.UI;

namespace BerserkV3.GameCore.EffectsVisual.Models
{
	public class RuntimeEffectModel : IRuntimeEffectModel
	{
		public int Id { get; set; }
		public int ExecutorId { get; set; }
		public int CurrentLength { get; set; }
		public int CurrentValue { get; set; }
		public int DisabledLength { get; set; }
		public List<int> AppliedIds { get; set; } = new();
		public EffectData Data { get; set; }
		public List<IEffectRuntimeArg> RuntimeArgs { get; set; } = new();
		public IRuntimeObjectView[] Targets { get; set; } = Array.Empty<IRuntimeObjectView>();
		public IRuntimeObjectView Executor { get; set; }
	}
}