using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.Utils
{
	public static class SharedEnumerableExtensions
	{
		public static bool TryGet<T>(this IEnumerable<T> source, Func<T, bool> predicate, out T result)
		{
			result = default;
			if (source == null || predicate == null)
				return false;

			result = source.FirstOrDefault(predicate);
			return result != null;
		}

		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
		{
			return source.Shuffle(new Random());
		}

		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (rng == null) throw new ArgumentNullException(nameof(rng));

			return source.ShuffleIterator(rng) ?? Array.Empty<T>();
		}

		private static IEnumerable<T> ShuffleIterator<T>(this IEnumerable<T> source, Random rng)
		{
			var buffer = source.ToList();
			for (var i = 0; i < buffer.Count; i++)
			{
				var j = rng.Next(i, buffer.Count);
				yield return buffer[j];

				buffer[j] = buffer[i];
			}
		}

		public static IEnumerable<IRuntimeGameCard> GetCardsFilterBy(
			this IEnumerable<IRuntimeGameObject> source,
			RuntimeState? state = null,
			string userId = "",
			ObjectType? type = null,
			bool? asQuery = null,
			params int[] runtimeIds)
		{
			var stateFilterDisabled = !state.HasValue;
			var query = source.GetObjectsFilterBy(userId, type, true, runtimeIds)
				.OfType<IRuntimeGameCard>()
				.Where(x => stateFilterDisabled || x.RuntimeData.State == state)
				.OrderBy(x => x.RuntimeData.RelativePositionX);
			
			return asQuery ?? false ? query : query.ToArray();
		}

		public static IEnumerable<IRuntimeGameObject> GetObjectsFilterBy(
			this IEnumerable<IRuntimeGameObject> source,
			string userId = "",
			ObjectType? type = null,
			bool asQuery = false,
			params int[] runtimeIds)
		{
			var typeFilterDisabled = !type.HasValue;
			var userIdFilerDisabled = string.IsNullOrEmpty(userId);
			var byIdFilterDisabled = runtimeIds.Length == 0;

			var query = source
				.Where(x => userIdFilerDisabled || x.RuntimeData.OwnerUserId == userId)
				.Where(x => typeFilterDisabled || type!.Value.HasFlag(x.Data.Type))
				.Where(x => byIdFilterDisabled || runtimeIds.Contains(x.RuntimeData.Id));

			return asQuery ? query : query.ToArray();
		}

		public static Dictionary<T1, T2> ZipToDictionary<T1, T2>(this IEnumerable<T1> keys, IEnumerable<T2> values)
		{
			return keys.Zip(values, (key, value) => (key, value))
				.ToDictionary(item => item.key, item => item.value);
		}

		public static IEnumerable<TResult> SelectWhere<TValue, TResult>(this IEnumerable<TValue> source, Func<TValue, (bool, TResult)> predicate)
		{
			foreach (var value in source)
			{
				var (condition, result) = predicate(value);
				if (condition)
					yield return result;
			}
		}
	}
}