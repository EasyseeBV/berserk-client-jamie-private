using System;
using System.Collections.Generic;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.GameCore.Abstraction
{
	public interface IRuntimeObject {}

	public interface IRuntimeGameObject : IEquatable<IRuntimeGameObject>, IDisposable, IRuntimeObject
	{
		event Action<EffectPhase, int, DamageType> OnPhaseChanged;
		event Action<IRuntimeEffect> OnBuffEffectDeleted;
		event Action<IRuntimeEffect> OnBuffEffectAdded;
		event Action<IRuntimeEffect> OnBuffEffectChanged;
		event Action<ImmuneKeyword> OnImmuneKeywordAdded;
		event Action<ImmuneKeyword> OnImmuneKeywordDeleted;
		event Action<DamageValue> OnImmuneDamageAdded;
		event Action<DamageValue> OnImmuneDamageDeleted;
		event Action<string[]> OnImpossingEffectDeleted;
		event Action<IntStat, int> OnRestore;
		event Action<IntStat, int> OnHit;
		event Action OnSpawn;
		event Action OnDie;
		
		List<IRuntimeEffect> AppliedEffects { get; }
		IRuntimeData RuntimeData { get; }
		IObjectData Data { get; }

		bool IsDead { get; }

		void Spawn(bool notify = true);

		void Attack(
			IRuntimeGameObject target,
			DamageType damageType, 
			ref DamageType counterDamageType,
			bool canHandleDie = true, 
			params EffectPhase[] exclude);

		void TakeDamage(
			int damage, 
			IRuntimeGameObject initiator, 
			DamageType damageType, 
			ref DamageType counterDamageType, 
			bool canHandleDie = true, 
			params EffectPhase[] exclude);

		void TakeRestore(int value, IRuntimeGameObject initiator, params EffectPhase[] exclude);
		
		void TryCounterAttack(IRuntimeGameObject target, ref DamageType counterDamageType, bool canHandleDie = true);

		/// <summary>
		/// Add applied effect if old same effect does not applied before,
		/// otherwise will stack with effect adn out 2 effects (stacks & execute)
		/// </summary>
		/// <param name="newEffect">new effect to add or stack</param>
		/// <param name="stackEffect">result of stacking</param>
		/// <param name="executeEffect">reorder execution if possible</param>
		void AddOrStackAppliedEffect(IRuntimeEffect newEffect,
			out IRuntimeEffect stackEffect,
			out IRuntimeEffect executeEffect);

		void ApplyAppliedEffect(IRuntimeEffect newEffect);

		void ApplyImmuneKeyword(ImmuneKeyword keyword);

		void ApplyImmuneDamage(DamageValue damageValue);

		void RemoveImmuneKeyword(ImmuneKeyword keyword);

		void RemoveImmuneDamage(DamageValue damageValue);

		void RemoveImpossingEffects(params string[] ids);

		bool RemoveAppliedEffect(object id);

		bool HasAppliedEffect(object id);
		
		bool HasAppliedNonDisabledEffect(object id);

		bool HasEffectsDisable();

		bool TryGetAppliedEffect(object id, out IRuntimeEffect result);

		void ProcessedDamageValue(ref int rawDamageValue, DamageType damageType);
		void SpendMove();
		void ResetEffects(params IRuntimeEffect[] except);
		void ResetStatsToDefault();
		void ResetBuffStats();
		void TryDie(IRuntimeGameObject initiator, DamageType damageType);
		void ChangedAppliedEffect(IRuntimeEffect changedEffect);
		void ChangeEffectPhase(EffectPhase phase, int? runtimeId = null, DamageType damageType = DamageType.None);
		bool CanAffect(EffectAffectionType afType, DamageType dmgType, out InvalidAction reason, params string[] keywords);
	}
}