using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.EffectSystem.TargetSystem
{
	public static class TargetFilterExtensions
	{
		public static IEnumerable<IRuntimeGameObject> ApplyParamFilter(
			this IRuntimeGameObject target,
			IRuntimeGameObject executor,
			ParamFilter paramFilter,
			int paramFilterValue)
		{
			return new[] {target}.ApplyParamFilter(executor, paramFilter, paramFilterValue);
		}
		public static IEnumerable<IRuntimeGameObject> ApplyParamFilter(
			this IEnumerable<IRuntimeGameObject> targets,
			IRuntimeGameObject executor,
			ParamFilter paramFilter, 
			int paramFilterValue)
		{
			return paramFilter switch
			{
				ParamFilter.None => targets,
				ParamFilter.ManaHighest => targets.GetFiltredUnitsByMana(false),
				ParamFilter.ManaSmallest => targets.GetFiltredUnitsByMana(true),
				
				ParamFilter.AttackHighest => targets.GetFiltredUnitsByAttack(false),
				ParamFilter.AttackSmallest => targets.GetFiltredUnitsByAttack(true),
				
				ParamFilter.HpHighest => targets.GetFiltredUnitsByHp(false),
				ParamFilter.HpSmallest => targets.GetFiltredUnitsByHp(true),
				ParamFilter.HpDamaged => targets.Where(x=> !x.RuntimeData.Hp.IsMax),
				
				ParamFilter.ArmorHighest => targets.GetFilterdUnitsByArmor(false),
				ParamFilter.ArmorSmallest => targets.GetFilterdUnitsByArmor(true),
				ParamFilter.ArmorDamaged => targets.Where(x=> !x.RuntimeData.Armor.IsMax),
				
				ParamFilter.ManaEqual => targets.Where(x => IsEqual(x.RuntimeData.Mana, paramFilterValue)),
				ParamFilter.ManaEqualExecutor => targets.Where(x => IsEqual(x.RuntimeData.Mana, executor.RuntimeData.Mana)),
				ParamFilter.ManaLess => targets.Where(x => IsLess(x.RuntimeData.Mana, paramFilterValue)),
				ParamFilter.ManaLessExecutor => targets.Where(x => IsLess(x.RuntimeData.Mana, executor.RuntimeData.Mana)),
				ParamFilter.ManaLessOrEqual => targets.Where(x => IsLess(x.RuntimeData.Mana, paramFilterValue) || IsEqual(x.RuntimeData.Mana, paramFilterValue)),
				ParamFilter.ManaLessOrEqualExecutor => targets.Where(x => IsLess(x.RuntimeData.Mana, executor.RuntimeData.Mana) || IsEqual(x.RuntimeData.Mana, executor.RuntimeData.Mana)),
				ParamFilter.ManaMore => targets.Where(x => IsMore(x.RuntimeData.Mana, paramFilterValue)),
				ParamFilter.ManaMoreExecutor => targets.Where(x => IsMore(x.RuntimeData.Mana, executor.RuntimeData.Mana)),
				ParamFilter.ManaMoreOrEqual => targets.Where(x => IsMore(x.RuntimeData.Mana, paramFilterValue) || IsEqual(x.RuntimeData.Mana, paramFilterValue)),
				ParamFilter.ManaMoreOrEqualExecutor => targets.Where(x => IsMore(x.RuntimeData.Mana, executor.RuntimeData.Mana) || IsEqual(x.RuntimeData.Mana, executor.RuntimeData.Mana)),
				
				ParamFilter.AttackEqual => targets.Where(x => IsEqual(x.RuntimeData.Attack, paramFilterValue)),
				ParamFilter.AttackEqualExecutor => targets.Where(x => IsEqual(x.RuntimeData.Attack, executor.RuntimeData.Attack)),
				ParamFilter.AttackLess => targets.Where(x => IsLess(x.RuntimeData.Attack, paramFilterValue)),
				ParamFilter.AttackLessExecutor => targets.Where(x => IsLess(x.RuntimeData.Attack, executor.RuntimeData.Attack)),
				ParamFilter.AttackLessOrEqual => targets.Where(x => IsLess(x.RuntimeData.Attack, paramFilterValue) || IsEqual(x.RuntimeData.Attack, paramFilterValue)),
				ParamFilter.AttackLessOrEqualExecutor => targets.Where(x => IsLess(x.RuntimeData.Attack, executor.RuntimeData.Attack) || IsEqual(x.RuntimeData.Attack, executor.RuntimeData.Attack)),
				ParamFilter.AttackMore => targets.Where(x => IsMore(x.RuntimeData.Attack, paramFilterValue)),
				ParamFilter.AttackMoreExecutor => targets.Where(x => IsMore(x.RuntimeData.Attack, executor.RuntimeData.Attack)),
				ParamFilter.AttackMoreOrEqual => targets.Where(x => IsMore(x.RuntimeData.Attack, paramFilterValue) || IsEqual(x.RuntimeData.Attack, paramFilterValue)),
				ParamFilter.AttackMoreOrEqualExecutor => targets.Where(x => IsMore(x.RuntimeData.Attack, executor.RuntimeData.Attack) || IsEqual(x.RuntimeData.Attack, executor.RuntimeData.Attack)),

				ParamFilter.HpEqual => targets.Where(x => IsEqual(x.RuntimeData.Hp, paramFilterValue)),
				ParamFilter.HpEqualExecutor => targets.Where(x => IsEqual(x.RuntimeData.Hp, executor.RuntimeData.Hp)),
				ParamFilter.HpLess => targets.Where(x => IsLess(x.RuntimeData.Hp, paramFilterValue)),
				ParamFilter.HpLessExecutor => targets.Where(x => IsLess(x.RuntimeData.Hp, executor.RuntimeData.Hp)),
				ParamFilter.HpLessOrEqual => targets.Where(x => IsLess(x.RuntimeData.Hp, paramFilterValue) || IsEqual(x.RuntimeData.Hp, paramFilterValue)),
				ParamFilter.HpLessOrEqualExecutor => targets.Where(x => IsLess(x.RuntimeData.Hp, executor.RuntimeData.Hp) || IsEqual(x.RuntimeData.Hp, executor.RuntimeData.Hp)),
				ParamFilter.HpMore => targets.Where(x => IsMore(x.RuntimeData.Hp, paramFilterValue)),
				ParamFilter.HpMoreExecutor => targets.Where(x => IsMore(x.RuntimeData.Hp, executor.RuntimeData.Hp)),
				ParamFilter.HpMoreOrEqual => targets.Where(x => IsMore(x.RuntimeData.Hp, paramFilterValue) || IsEqual(x.RuntimeData.Hp, paramFilterValue)),
				ParamFilter.HpMoreOrEqualExecutor => targets.Where(x => IsMore(x.RuntimeData.Hp, executor.RuntimeData.Hp) || IsEqual(x.RuntimeData.Hp, executor.RuntimeData.Hp)),

				ParamFilter.ArmorEqual => targets.Where(x => IsEqual(x.RuntimeData.Armor, paramFilterValue)),
				ParamFilter.ArmorEqualExecutor => targets.Where(x => IsEqual(x.RuntimeData.Armor, executor.RuntimeData.Armor)),
				ParamFilter.ArmorLess => targets.Where(x => IsLess(x.RuntimeData.Armor, paramFilterValue)),
				ParamFilter.ArmorLessExecutor => targets.Where(x => IsLess(x.RuntimeData.Armor, executor.RuntimeData.Armor)),
				ParamFilter.ArmorLessOrEqual => targets.Where(x => IsLess(x.RuntimeData.Armor, paramFilterValue) || IsEqual(x.RuntimeData.Armor, paramFilterValue)),
				ParamFilter.ArmorLessOrEqualExecutor => targets.Where(x => IsLess(x.RuntimeData.Armor, executor.RuntimeData.Armor) || IsEqual(x.RuntimeData.Armor, executor.RuntimeData.Armor)),
				ParamFilter.ArmorMore => targets.Where(x => IsMore(x.RuntimeData.Armor, paramFilterValue)),
				ParamFilter.ArmorMoreExecutor => targets.Where(x => IsMore(x.RuntimeData.Armor, executor.RuntimeData.Armor)),
				ParamFilter.ArmorMoreOrEqual => targets.Where(x => IsMore(x.RuntimeData.Armor, paramFilterValue) || IsEqual(x.RuntimeData.Armor, paramFilterValue)),
				ParamFilter.ArmorMoreOrEqualExecutor => targets.Where(x => IsMore(x.RuntimeData.Armor, executor.RuntimeData.Armor) || IsEqual(x.RuntimeData.Armor, executor.RuntimeData.Armor)),

				_ => throw new ArgumentOutOfRangeException(nameof(paramFilter), paramFilter, null)
			};
		}
		
		public static bool IsEqual(this IntStat stat, int value)
		{
			return stat == value;
		}

		public static bool IsLess(this IntStat stat, int value)
		{
			return stat < value;
		}
		
		public static bool IsMore(this IntStat stat, int value)
		{
			return stat > value;
		}

		public static IEnumerable<IRuntimeGameObject> GetFiltredUnitsByMana(this IEnumerable<IRuntimeGameObject> units, bool isLowest)
		{
			return GetExtremumUnits(units, unit => unit.RuntimeData.Mana, isLowest);
		}

		public static IEnumerable<IRuntimeGameObject> GetFiltredUnitsByAttack(this IEnumerable<IRuntimeGameObject> units, bool isLowest)
		{
			return GetExtremumUnits(units, unit => unit.RuntimeData.Attack, isLowest);
		}

		public static IEnumerable<IRuntimeGameObject> GetFiltredUnitsByHp(this IEnumerable<IRuntimeGameObject> units, bool isLowest)
		{
			return GetExtremumUnits(units, unit => unit.RuntimeData.Hp, true);
		}

		public static IEnumerable<IRuntimeGameObject> GetFilterdUnitsByArmor(this IEnumerable<IRuntimeGameObject> units, bool isLowest)
		{
			return GetExtremumUnits(units, unit => unit.RuntimeData.Armor, isLowest);
		}

		public static IEnumerable<IRuntimeGameObject> GetExtremumUnits(
			this IEnumerable<IRuntimeGameObject> units,
			Func<IRuntimeGameObject, IntStat> statSelector, 
			bool isLowest)
		{
			var runtimeGameObjects = units?.ToArray() ?? Array.Empty<IRuntimeGameObject>();
			if (runtimeGameObjects.Length == 0)
				return Array.Empty<IRuntimeGameObject>();
			
			var extremumStat = isLowest
				? runtimeGameObjects.Min(statSelector)
				: runtimeGameObjects.Max(statSelector);

			return runtimeGameObjects.Where(x => statSelector(x) == extremumStat!);
		}

		public static T FirstRandomOrDefault<T>(this IEnumerable<T> targets)
		{
			return targets != null 
				? targets.Shuffle().FirstOrDefault() 
				: default;
		}

		public static int GetMaxTargetCount(this EffectData source)
		{
			return source == null ? int.MaxValue : source.Aoe.GetMaxTargetCount(source.MaxTargetCount);
		}

		public static int GetMaxTargetCount(this EffectAoe source, int count)
		{
			return source switch
			{
				EffectAoe.Single => 1,
				EffectAoe.Many => count <= 0 ? int.MaxValue : count,
				_ => throw new NotImplementedException($"[{nameof(TargetFilterExtensions)}.{nameof(GetMaxTargetCount)}] Unknown {nameof(EffectAoe)}")
			};
		}

		public static T[] TakeNRandom<T>(this IEnumerable<T> source, int maxCount)
		{
			return source == null || maxCount <= 0 ? Array.Empty<T>() : source.Shuffle().Take(maxCount).ToArray();
		}
	}
}