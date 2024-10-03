using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicContext;

namespace Berserk.Shared.GameCore.Utils
{
	public static class EffectsExtensions
	{
		public static string AsEffectId(this EffectKeyword value)
		{
			return value.ToString().ToLower();
		}

		public static string AsRuntimeEffectId(this EffectKeyword value)
		{
			return $"runtime-{value}";
		}

		public static string AsSystemEffectId(this EffectKeyword value)
		{
			return $"system-{value}";
		}

		public static IntStat GetStatByName(this IRuntimeData data, string statName)
		{
			return statName switch
			{
				nameof(data.Attack) => data.Attack,
				nameof(data.Hp) => data.Hp,
				nameof(data.Mana) => data.Mana,
				nameof(data.Armor) => data.Armor,
				nameof(data.MoveCount) => data.MoveCount,
				_ => (data as IRuntimeHeroData)?.GetStatByName(statName)
				     ?? throw new InvalidOperationException($"Stat name does not exist : {statName}")
			};
		}

		public static IntStat GetStatByName(this IRuntimeHeroData data, string statName)
		{
			return statName switch
			{
				nameof(data.AbilityMoveCount) => data.AbilityMoveCount,
				_ => throw new InvalidOperationException($"Stat name does not exist : {statName}")
			};
		}

		public static IntStat GetStatByName(this IRuntimeGameObject target, string statName)
		{
			return GetStatByName(target.RuntimeData, statName);
		}

		public static IEnumerable<IntStat> GetStatsFromMeta(this EffectData effectData, IRuntimeGameObject target)
		{
			return effectData.Meta
				.Split(',', StringSplitOptions.RemoveEmptyEntries)
				.Select(target.GetStatByName)
				.ToArray();
		}

		public static string[] GetConfigIdsFromMeta(this EffectData effectData)
		{
			return effectData.Meta.GetConfigIds();
		}

		public static string[] GetConfigIds(this string meta)
		{
			if (string.IsNullOrEmpty(meta))
			{
				DefaultSharedLogger.Error("Meta does not have any effect config id.");
				return Array.Empty<string>();
			}

			return meta
				.Split(',', StringSplitOptions.RemoveEmptyEntries)
				.ToArray();
		}

		public static IEnumerable<KeyValuePair<string, IntStat>> GetStatsKeyValueFromMeta(this EffectData effectData,
			IRuntimeGameObject target)
		{
			return effectData.Meta
				.Split(',', StringSplitOptions.RemoveEmptyEntries)
				.Select(id => KeyValuePair.Create(id, target.GetStatByName(id)))
				.ToArray();
		}
	}
}