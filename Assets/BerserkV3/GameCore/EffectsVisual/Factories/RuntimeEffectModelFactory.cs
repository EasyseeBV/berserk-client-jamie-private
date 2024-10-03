using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Game;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.GameCore.EffectsVisual.Abstractions;
using BerserkV3.GameCore.EffectsVisual.Models;
using BerserkV3.GameCore.Repository;
using BerserkV3.GameCore.UI;
using RR.Core.Extensions;

namespace BerserkV3.GameCore.EffectsVisual.Factories
{
	public class RuntimeEffectModelFactory : IRuntimeEffectModelFatory
	{
		private readonly IGameContext gameContext;
		private readonly IGameRepository gameRepository;

		public RuntimeEffectModelFactory(
			IGameContext gameContext,
			IGameRepository gameRepository)
		{
			this.gameContext = gameContext;
			this.gameRepository = gameRepository;
		}

		public IRuntimeEffectModel Create(
			EffectData data,
			IRuntimeObjectView executor,
			IEnumerable<IEffectRuntimeArg> args = null,
			params IRuntimeObjectView[] targets)
		{
			return new RuntimeEffectModel
			{
				Data = data,
				CurrentValue = data.Value,
				CurrentLength = data.Length,
				Executor = executor,
				Targets = targets,
				ExecutorId = executor?.RuntimeData?.Id ?? int.MinValue,
				RuntimeArgs = args?.ToList() ?? new List<IEffectRuntimeArg>(),
				Id = int.MinValue
			};
		}

		public IRuntimeEffectModel Create(IRuntimeEffectData runtimeData)
		{
			if (runtimeData == null)
				throw new NullReferenceException($"[{GetType().Name.Orange()}] Can't create {nameof(IRuntimeEffectModel)}, {nameof(IRuntimeEffectData)} is missing.");

			var data = gameContext.GameDatabase.GetEffectConfig(runtimeData.ConfigId);
			if (data == null)
				throw new NullReferenceException($"[{GetType().Name.Orange()}] Can't create {nameof(IRuntimeEffectModel)}, {nameof(EffectData)} not found im {nameof(IGameDatabase)}");

			return Create(runtimeData, data);
		}

		public IRuntimeEffectModel Create(IRuntimeEffectData runtimeData, EffectData data)
		{
			return new RuntimeEffectModel
			{
				Id = runtimeData.Id,
				ExecutorId = runtimeData.ExecutorId,
				CurrentLength = runtimeData.CurrentLength,
				DisabledLength = runtimeData.DisabledLength,
				CurrentValue = runtimeData.CurrentValue,
				AppliedIds = runtimeData.AppliedIds.ToList(),
				Data = data,
				RuntimeArgs = runtimeData.RuntimeArgs.ToList(),
				Targets = runtimeData.TargetIds.Select(gameRepository.GetObjectViewByRuntimeId).ToArray(),
				Executor = gameRepository.GetObjectViewByRuntimeId(runtimeData.ExecutorId),
			};
		}
	}
}