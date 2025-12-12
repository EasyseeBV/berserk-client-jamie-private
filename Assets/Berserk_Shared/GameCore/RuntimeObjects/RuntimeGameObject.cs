using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.RuntimeObjects
{
	public abstract class RuntimeGameObject : IRuntimeGameObject
	{
		public event Action<EffectPhase, int, DamageType> OnPhaseChanged;
		public event Action<IRuntimeEffect> OnBuffEffectDeleted;
		public event Action<IRuntimeEffect> OnBuffEffectAdded;
		public event Action<IRuntimeEffect> OnBuffEffectChanged;
		public event Action<ImmuneKeyword> OnImmuneKeywordAdded;
		public event Action<ImmuneKeyword> OnImmuneKeywordDeleted;
		public event Action<DamageValue> OnImmuneDamageAdded;
		public event Action<DamageValue> OnImmuneDamageDeleted;
		public event Action<string[]> OnImpossingEffectDeleted;
		public event Action<IStat<int>> OnRestore;
		public event Action<IStat<int>> OnHit;
		public event Action OnSpawn;
		public event Action OnDie;

		public IList<IRuntimeEffect> AppliedEffects { get;  private set; }
		public IRuntimeData RuntimeData { get; private set;  }
		public IObjectData Data { get; private set; }

		public abstract bool IsDead { get; }
		
		public IRuntimeGameObject Init(
			IRuntimeData runtimeData, 
			IObjectData objectData)
		{
			AppliedEffects = new List<IRuntimeEffect>();
			RuntimeData = runtimeData;
			Data = objectData;
			return this;
		}

		public virtual void Spawn(bool notify = true)
		{
			if (notify)
				OnSpawn?.Invoke();
			ChangeEffectPhase(EffectPhase.AfterSpawn);
		}
		
		public void SpendMove()
		{
			var moveCount = Math.Max(0, RuntimeData.MoveCount - 1);
			RuntimeData.MoveCount.Set(moveCount);
		}
		
		public virtual void Attack(
			IRuntimeGameObject target, 
			DamageType damageType, 
			ref DamageType counterDamageType,
			bool canHandleDie = true, 
			params EffectPhase[] exclude)
		{
			if (target.IsDead)
			{
				DefaultSharedLogger.Log($"Trying to attack dead object DataId : {target.Data.Id} : runtimeId :{target.RuntimeData.Id}");
				return;
			}
			
			// `BeforeAttack` is invoked before any actions connected to the attack
			if (!exclude.Contains(EffectPhase.BeforeAttack))
				ChangeEffectPhase(EffectPhase.BeforeAttack, target.RuntimeData.Id, damageType);

			int attackValue = RuntimeData.Attack;
			if (target.TryGetAppliedEffect(EffectKeyword.Fortitude, out var fortitude) && attackValue <= fortitude.RuntimeData.CurrentValue)
				attackValue = 0;
			
			// When attacking, we don't need canHandleDie because we handle them later in the Attack method
			target.TakeDamage(attackValue, this, damageType, ref counterDamageType, false, exclude);

			if (!exclude.Contains(EffectPhase.AfterAttack))
				ChangeEffectPhase(EffectPhase.AfterAttack, target.RuntimeData.Id, damageType);

			if (canHandleDie)
			{
				target.TryDie(this, damageType);
				TryDie(target, counterDamageType);
			}
		}

		public virtual void TakeDamage(
			int damage, 
			IRuntimeGameObject initiator, 
			DamageType damageType, 
			ref DamageType counterDamageType,
			bool canHandleDie = true, 
			params EffectPhase[] exclude)
		{
			if (IsDead)
			{
				DefaultSharedLogger.Log($"Trying to attack dead object DataId : {Data.Id} : runtimeId :{RuntimeData.Id}");
				return;
			}
			
			if (HasAppliedNonDisabledEffect(EffectKeyword.ImmortalOneHit))
			{
				RemoveAppliedEffect(EffectKeyword.ImmortalOneHit);
				damage = 0;
			}
			
			if (damageType == DamageType.Direct && !exclude.Contains(EffectPhase.BeforeDefense))
				ChangeEffectPhase(EffectPhase.BeforeDefense, initiator.RuntimeData.Id, damageType);

			ProcessedDamageValue(ref damage, damageType);
			if (damage > 0 && !exclude.Contains(EffectPhase.BeforeDamaged))
				ChangeEffectPhase(EffectPhase.BeforeDamaged, initiator.RuntimeData.Id, damageType);

			HandleDamage(damage, initiator, damageType);

			if (!exclude.Contains(EffectPhase.Counterattack))
				TryCounterAttack(initiator, ref counterDamageType, canHandleDie);

			if (damage > 0 && !exclude.Contains(EffectPhase.AfterDamaged))
				ChangeEffectPhase(EffectPhase.AfterDamaged, initiator.RuntimeData.Id, damageType);

			if (damageType == DamageType.Direct && !exclude.Contains(EffectPhase.AfterDefense))
				ChangeEffectPhase(EffectPhase.AfterDefense, initiator.RuntimeData.Id, damageType);

			if (canHandleDie)
				TryDie(initiator, damageType);
		}

		public void TakeRestore(int value, IRuntimeGameObject initiator, params EffectPhase[] exclude)
		{
			if (value <= 0)
				return;
			
			var stat = GetRestoreStat(value, initiator);
			stat.Add(value);
			OnRestore?.Invoke(stat);
			
			if (!exclude.Contains(EffectPhase.AfterHeal))
				ChangeEffectPhase(EffectPhase.AfterHeal);
		}

		public virtual void TryCounterAttack(
			IRuntimeGameObject target, 
			ref DamageType counterDamageType,  
			bool canHandleDie = true)
		{
			if (HasAppliedNonDisabledEffect(EffectKeyword.Stunning))
				return;
			
			if (!target.CanAffect(EffectAffectionType.Neutral, DamageType.CounterAttack, out _))
				return;

			counterDamageType = DamageType.CounterAttack;
			target.TakeDamage(RuntimeData.Attack, this, DamageType.CounterAttack, ref counterDamageType, canHandleDie, EffectPhase.Counterattack);
		}
		
		public bool CanAffect(
			EffectAffectionType afType, 
			DamageType dmgType,
			out InvalidAction reason,
			params string[] keywords)
		{
			reason = InvalidAction.None;
			
			if (HasEffectsDisable())
				return false;
			
			if (HasAppliedNonDisabledEffect(EffectKeyword.Petrify))
				return false;

			if ((DamageType.CounterAttack == dmgType || afType == EffectAffectionType.Debuff)
			    && HasAppliedNonDisabledEffect(EffectKeyword.Immortal))
			{
				reason = InvalidAction.Immune;
				return false;
			}

			// if (RuntimeData.ImmuneToDamage.TryGet(o => o.DamageType == dmgType, out var damageValue)
			//     && damageValue.IsFullResist())
			// {
			// 	reason = InvalidAction.ImmuneToDamage;
			// 	return false;
			// }

			if (keywords.Length > 0 && RuntimeData.ImmuneToKeywords.Any(data => keywords.Contains(data.Keyword)))
			{
				reason = InvalidAction.ImmuneToKeyword;
				return false;
			}

			return true;
		}

		public virtual bool IsDeadPrevented()
		{
			return HasAppliedNonDisabledEffect(EffectKeyword.Barricade);
		}
		
		public void TryDie(IRuntimeGameObject initiator, DamageType damageType)
		{
			if (!IsDead)
				return;
			
			initiator.ChangeEffectPhase(EffectPhase.AfterKill, RuntimeData.Id, damageType);

			ChangeEffectPhase(EffectPhase.BeforeDead, initiator.RuntimeData.Id, damageType);

			if (!IsDeadPrevented())
			{
				OnDie?.Invoke();
				OnDied();
			}
			
			ChangeEffectPhase(EffectPhase.AfterDeath, initiator.RuntimeData.Id, damageType);
		}

		public void ChangedAppliedEffect(IRuntimeEffect effect)
		{
			if (!HasAppliedEffect(effect))
				throw new InvalidOperationException("You are trying to change the effect, but the RuntimeGameObject has no such effect.");

			RuntimeData.AppliedEffects.RemoveAll(x => x.Id == effect.RuntimeData.Id);
			RuntimeData.AppliedEffects.Add(effect.RuntimeData);
			OnBuffEffectChanged?.Invoke(effect);
			effect.OnChanged();
		}

		public void ProcessedDamageValue(ref int rawDamageValue, DamageType damageType)
		{
			if (rawDamageValue <= 0)
			{
				rawDamageValue = 0;
				return;
			}
			
			rawDamageValue = damageType switch
			{
				DamageType.None or DamageType.Direct or DamageType.CounterAttack =>
					RuntimeData.Armor > 0 ? 1 : rawDamageValue,
				
				DamageType.Pure or DamageType.Spell => rawDamageValue,
				
				_ => throw new NotImplementedException($"Unknown {nameof(DamageType)}: {damageType}")
			};
			
			if (RuntimeData.ImmuneToDamage.TryGet(o => o.DamageType == damageType, out var damageValue))
				rawDamageValue = damageValue.GetDamage(rawDamageValue);
		}

		public void RemoveImmuneDamage(DamageValue damageValue)
		{
			if (RuntimeData.ImmuneToDamage.Remove(damageValue))
				OnImmuneDamageDeleted?.Invoke(damageValue);
		}
		
		public void RemoveImmuneKeyword(ImmuneKeyword keyword)
		{
			if (RuntimeData.ImmuneToKeywords.Remove(keyword))
				OnImmuneKeywordDeleted?.Invoke(keyword);
		}
		
		public void RemoveImpossingEffects(params string[] ids)
		{
			if (RuntimeData.ImposingEffects.RemoveAll(ids.Contains) > 0)
				OnImpossingEffectDeleted?.Invoke(ids);
		}

		public bool RemoveAppliedEffect(object id)
		{
			if (!TryGetAppliedEffect(id, out var appliedEffect))
				return false;
			
			AppliedEffects.Remove(appliedEffect);
			RuntimeData.AppliedEffects.Remove(appliedEffect.RuntimeData);
			
			appliedEffect.Expire();
			OnBuffEffectDeleted?.Invoke(appliedEffect);
			appliedEffect.OnDeleted();
			return true;
		}
		
		public void AddOrStackAppliedEffect(
			IRuntimeEffect newEffect, 
			out IRuntimeEffect stackEffect, 
			out IRuntimeEffect executeEffect)
		{
			if (!newEffect.EffectData.EffectStacks.Contains(EffectStack.DoNotStack))
			{
				stackEffect = AppliedEffects.ToArray()
					.FirstOrDefault(e => e.EffectData.Keyword == newEffect.EffectData.Keyword 
					                     && !e.EffectData.EffectStacks.Contains(EffectStack.DoNotStack));
				if (stackEffect != null)
				{
					executeEffect = stackEffect.Stack(newEffect);
					ChangedAppliedEffect(stackEffect);
					return;
				}
			}

			stackEffect = executeEffect = newEffect;
			AppliedEffects.Add(newEffect);
			RuntimeData.AppliedEffects.Add(newEffect.RuntimeData);
			OnBuffEffectAdded?.Invoke(newEffect);
			newEffect.OnAdded();
		}
		
		public void ApplyAppliedEffect(IRuntimeEffect newEffect)
		{
			AddOrStackAppliedEffect(newEffect, out _, out _);
		}

		public void ApplyImmuneKeyword(ImmuneKeyword keyword)
		{
			RuntimeData.ImmuneToKeywords.Add(keyword);
			OnImmuneKeywordAdded?.Invoke(keyword);
		}

		public void ApplyImmuneDamage(DamageValue damageValue)
		{
			RuntimeData.ImmuneToDamage.Add(damageValue);
			OnImmuneDamageAdded?.Invoke(damageValue);
		}

		public bool TryGetAppliedEffect(object id, out IRuntimeEffect result)
		{
			switch (id)
			{
				case int runtimeId:
					result = AppliedEffects.FirstOrDefault(effect => effect.RuntimeData.Id == runtimeId);
					return result != null;
				
				case string effectConfigId:
					result = AppliedEffects.FirstOrDefault(effect => effect.RuntimeData.ConfigId == effectConfigId);
					
					if (result == null && Enum.TryParse(effectConfigId, out EffectKeyword parsedKeyword))
						return TryGetAppliedEffect(parsedKeyword, out result);
					
					return result != null;
				
				case EffectKeyword keyword:
					result = AppliedEffects.FirstOrDefault(effect => effect.EffectData.Keyword == keyword);
					return result != null;
				
				case IRuntimeEffect incoming:
					result = AppliedEffects.FirstOrDefault(effect => effect.RuntimeData.Id == incoming.RuntimeData.Id);
					return result != null;
				
				default: 
					result = default;
					DefaultSharedLogger.Error($"[{nameof(TryGetAppliedEffect)}] Not Implemented object id : {id}");
					return false;
			}
		}

		public bool HasAppliedEffect(object id)
		{
			return TryGetAppliedEffect(id, out _);
		}

		public bool HasAppliedNonDisabledEffect(object id)
		{
			return TryGetAppliedEffect(id, out var effect) && !effect.RuntimeData.Disabled;
		}
		
		public bool HasEffectsDisable()
		{
			return AppliedEffects.Any(effect => effect.EffectData.Keyword 
				is EffectKeyword.Barricade 
				or EffectKeyword.Stasis);
		}
		
		public virtual void Dispose()
		{
			RuntimeData?.Dispose();
			foreach (var effect in AppliedEffects.ToArray())
			{
				effect?.Dispose();
			}
			AppliedEffects.Clear();
			OnPhaseChanged = null;
			OnBuffEffectDeleted = null;
			OnBuffEffectAdded = null;
			OnBuffEffectChanged = null;
			OnRestore = null;
			OnHit = null;
			OnDie = null;
			OnSpawn = null;
		}

		public void ResetEffects(params IRuntimeEffect[] except)
		{
			foreach (var effect in AppliedEffects.Except(except).ToArray())
			{
				RemoveAppliedEffect(effect);
			}

			foreach (var damage in RuntimeData.ImmuneToDamage.ToArray())
			{
				if (except.All(x=> x.RuntimeData.Id != damage.EffectId))
					RemoveImmuneDamage(damage);
			}

			foreach (var keyword in RuntimeData.ImmuneToKeywords.ToArray())
			{
				if (except.All(x=> x.RuntimeData.Id != keyword.EffectId))
					RemoveImmuneKeyword(keyword);
			}
		}

		public virtual void ResetBuffStats()
		{
			ResetStat(RuntimeData.Mana, Data.Mana);
			ResetStat(RuntimeData.Hp, Data.Hp);
			ResetStat(RuntimeData.Attack, Data.Attack);
			ResetStat(RuntimeData.Armor, RuntimeData.Armor.BaseStat);
		}

		protected virtual void HandleDamage(int damage, IRuntimeGameObject initiator, DamageType damageType)
		{
			if (damage <= 0)
			{
    			OnHit?.Invoke(RuntimeData.Hp);
    			return;
			}
			
			if (RuntimeData.Armor.Current > 0)
			{
				if (damageType == DamageType.Pure)
				{
					RuntimeData.Hp.Substract(damage);
					OnHit?.Invoke(RuntimeData.Hp);
				}
				

				RuntimeData.Armor.Substract(damage);
				OnHit?.Invoke(RuntimeData.Armor);
				return;
			}
			
			RuntimeData.Hp.Substract(damage);
			OnHit?.Invoke(RuntimeData.Hp);
		}

		protected virtual IntStat GetRestoreStat(int value, IRuntimeGameObject initiator)
		{
			return RuntimeData.Hp;
		}
		
		protected void ResetStat(IntStat stat, int defaultStat, bool? resetToMax = null)
		{
			stat.ClearModifiers(false);
			resetToMax ??= stat.Current > defaultStat;
			stat.SetMax(defaultStat);
			
			if (resetToMax.Value)
			{
				stat.ResetToMax();
				return;
			}
			
			stat.NotifyChanges();
		}

		public virtual void ResetStatsToDefault()
		{
			ResetStat(RuntimeData.Mana, Data.Mana, true);
			ResetStat(RuntimeData.Hp, Data.Hp, true);
			ResetStat(RuntimeData.Attack, Data.Attack, true);
			ResetStat(RuntimeData.Armor, RuntimeData.Armor.BaseStat, true);
		}

		public bool Equals(IRuntimeGameObject other)
		{
			if (ReferenceEquals(other, null))
				return false;

			if (ReferenceEquals(this, other))
				return true;

			return RuntimeData.Equals(other.RuntimeData);
		}

		public void ChangeEffectPhase(EffectPhase phase, int? runtimeId = null, DamageType damageType = DamageType.None)
		{
			OnPhaseChanged?.Invoke(phase, runtimeId ?? RuntimeData.Id, damageType);
		}
		
		protected virtual void OnDied() {}
	}
}
