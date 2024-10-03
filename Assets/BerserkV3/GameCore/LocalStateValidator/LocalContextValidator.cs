using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.LogicEvents;
using RR.Core.Extensions;

namespace BerserkV3.GameCore.LocalStateValidator
{
	public interface ILocalContextValidator
	{
		bool Validate(IList<ILogicEvent> events);
	}

	public class LocalContextValidator : ILocalContextValidator
	{
		private readonly IGameContext gameContext;

		public LocalContextValidator(IGameContext gameContext)
		{
			this.gameContext = gameContext;
		}

		public bool Validate(IList<ILogicEvent> events)
		{
			var remote = events?.OfType<InitializeGame>().LastOrDefault();
			if (remote == null)
				return true;
			
			events.Remove(remote);
			events.Insert(0, remote);
			
			return remote.GameRuntimeDatas.All(ValidateRuntimeData)
			       && remote.GameRuntimeDatas.SelectMany(x=> x.AppliedEffects).All(ValidateEffect);
		}

		private bool ValidateRuntimeData(IRuntimeData remote)
		{
			try
			{
				if (!gameContext.GameRuntimePool.TryGet(remote.Id, out var localObj))
					CompareLog(null, remote);
				
				var local = localObj.RuntimeData;
				CompareLog(local.DataId, remote.DataId, "DataId");
				CompareLog(local.OwnerUserId, remote.OwnerUserId, "OwnerUserId");
				CompareLog(local.Hp, remote.Hp, "Hp");
				CompareLog(local.Mana, remote.Mana, "Mana");
				CompareLog(local.Attack, remote.Attack, "Attack");
				CompareLog(local.Armor, remote.Armor, "Armor");
				CompareLog(local.MoveCount, remote.MoveCount, "MoveCount");
				CompareLog(local.ImposingEffects.Count, remote.ImposingEffects.Count, "ImposingEffects.Count");
				CompareLog(local.ImposingEffects.JoinToString(), remote.ImposingEffects.JoinToString(), "ImposingEffects");
				CompareLog(local.ImmuneToKeywords.Count, remote.ImmuneToKeywords.Count, "ImmuneToEffects.Count");
				CompareLog(local.ImmuneToKeywords.JoinToString(), remote.ImmuneToKeywords.JoinToString(), "ImmuneToEffects");
				CompareLog(local.ImmuneToDamage.Count, remote.ImmuneToDamage.Count, "ImmuneToDamage.Count");
				CompareLog(local.ImmuneToDamage.JoinToString(), remote.ImmuneToDamage.JoinToString(), "ImmuneToDamage");

				if (local is IRuntimeHeroData localHero && remote is IRuntimeHeroData remoteHero)
					CompareLog(localHero.AbilityMoveCount, remoteHero.AbilityMoveCount, "AbilityMoveCount");
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
				return false;
			}

			return true;
		}

		private bool ValidateEffect(IRuntimeEffectData remote)
		{
			try
			{
				if (!gameContext.GameRuntimePool.TryGet(remote.ExecutorId, out var localObj))
					CompareLog("Local Runtime Object not found", remote.ExecutorId, "IRuntimeGameObject");
				
				if (!localObj.TryGetAppliedEffect(remote.Id, out var local))
					CompareLog("Local Runtime Effect not found", remote.Id, "IRuntimeEffect");
				
				CompareLog(local.RuntimeData.CurrentLength, remote.CurrentLength, "CurrentLength");
				CompareLog(local.RuntimeData.DisabledLength, remote.DisabledLength, "CurrentLength");
				CompareLog(local.RuntimeData.CurrentValue, remote.CurrentValue, "CurrentValue");
				CompareLog(local.EffectData.Id, remote.ConfigId, "ConfigId");
				CompareLog(local.Executor.RuntimeData.Id, remote.ExecutorId, "ExecutorId");
				CompareLog(local.Targets.Select(x=> x.RuntimeData.Id).JoinToString(), remote.TargetIds.JoinToString(), "TargetIds");
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
				return false;
			}

			return true;
		}

		private void CompareLog(object local, object remote, string compareName = "")
		{
			if (local?.ToString() == remote?.ToString())
				return;
			
			throw new Exception($"[{GetType().Name.Orange().Bold()}] : Cannot validate {compareName} : Local {local}, Remote {remote}");
		}
	}
}