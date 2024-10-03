using System;
using System.Collections.Generic;
using System.Linq;
using RR.Core.Serialization;
using Random = UnityEngine.Random;

namespace RR.Core.Extensions
{
	public static class EnumerableExtensions
	{
		public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, int> action)
		{
			var i = 0;
			foreach (var element in enumerable)
			{
				action(element, i);
				++i;
			}
		}

		public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
		{
			foreach (var element in enumerable)
			{
				action(element);
			}
		}

		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> enumerable, System.Random random)
			=> enumerable.OrderBy(a => random.Next());

		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> enumerable)
			=> enumerable.OrderBy(a => Guid.NewGuid());

		public static T RandomElement<T>(this IEnumerable<T> enumerable, System.Random random)
		{
			int count = enumerable.Count();
			return enumerable.ElementAt(random.Next(0, count));
		}

		public static T RandomElement<T>(this T[] array, System.Random random)
		{
			return array[random.Next(0, array.Length)];
		}

		public static T RandomElement<T>(this IEnumerable<T> enumerable)
		{
			int count = enumerable.Count();
			return enumerable.ElementAt(Random.Range(0, count));
		}

		public static T RandomElement<T>(this T[] array)
		{
			return array[Random.Range(0, array.Length)];
		}

		public static bool IsSubsetOf<T>(this IEnumerable<T> subset, IEnumerable<T> main)
		{
			return !subset.Except(main).Any();
		}

		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable)
		{
			return new HashSet<T>(enumerable);
		}

		public static int IndexOf<T>(this IList<T> list, Func<T, bool> predicate)
		{
			for (var i = 0; i < list.Count; i++)
				if (predicate(list[i]))
					return i;

			return -1;
		}

		public static T Add<T>(this ICollection<T> list, T value)
		{
			list.Add(value);
			return value;
		}

		/// <summary>
		/// Converts an <see cref="Array"/> into <see cref="List{T}"/>
		/// </summary>
		/// <param name="array">Array to convert</param>
		/// <typeparam name="T">Target list element type</typeparam>
		/// <exception cref="InvalidCastException">If elements of the array are not of type <see cref="T"/></exception>
		/// <returns></returns>
		public static List<T> ToList<T>(this Array array)
		{
			var list = new List<T>(array.Length);
			foreach (var element in array)
				list.Add((T)element);

			return list;
		}

		public static TSource FirstMin<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer = null)
			=> source.FirstMinMax(selector, comparer, true, false);

		public static TSource FirstMax<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer = null)
			=> source.FirstMinMax(selector, comparer, false, false);

		public static TSource FirstMinOrDefault<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer = null)
			=> source.FirstMinMax(selector, comparer, true, true);

		public static TSource FirstMaxOrDefault<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer = null)
			=> source.FirstMinMax(selector, comparer, false, true);

		private static TSource FirstMinMax<TSource, TKey>(
			this IEnumerable<TSource> source,
			Func<TSource, TKey> selector,
			IComparer<TKey> comparer,
			bool seekMin, bool orDefault)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (selector == null) throw new ArgumentNullException(nameof(selector));
			if (comparer == null) comparer = Comparer<TKey>.Default;

			bool Compare(TKey candidateProjected, TKey key) => seekMin
				? comparer.Compare(candidateProjected, key) >= 0
				: comparer.Compare(candidateProjected, key) < 0;

			using (var sourceIterator = source.GetEnumerator())
			{
				if (!sourceIterator.MoveNext())
				{
					if (orDefault) return default;
					throw new InvalidOperationException("Sequence reached end or contains no elements");
				}

				var key = selector(sourceIterator.Current);

				return IterateWithCompare(selector, x => Compare(x, key), sourceIterator);
			}
		}
		public static IEnumerable<T> GetFlags<T>(this T input, Func<T, bool> comparer = null) where T : struct, Enum
		{
			if (comparer == null) comparer = x => input.HasFlag(x);

			foreach (T value in Enum.GetValues(input.GetType()))
				if (comparer(value))
					yield return value;
		}

		private static TSource IterateWithCompare<TSource, TKey>(
			Func<TSource, TKey> selector,
			Func<TKey, bool> comparer,
			IEnumerator<TSource> sourceIterator)
		{
			var current = sourceIterator.Current;

			while (sourceIterator.MoveNext())
			{
				var candidate = sourceIterator.Current;
				var candidateProjected = selector(candidate);
				if (comparer(candidateProjected))
					continue;

				current = candidate;
			}

			return current;
		}

		public static UnitySerializedDictionary<TKey, TElement> ToSerializableDictionary<TSource, TKey, TElement>(
			this IEnumerable<TSource> source,
			Func<TSource, TKey> keySelector,
			Func<TSource, TElement> elementSelector)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			if (keySelector == null)
				throw new ArgumentNullException(nameof(keySelector));
			if (elementSelector == null)
				throw new ArgumentNullException(nameof(elementSelector));

			var dictionary = new UnitySerializedDictionary<TKey, TElement>();
			foreach (var item in source)
				dictionary.Add(keySelector(item), elementSelector(item));

			return dictionary;
		}

		public static string JoinToString<T>(this IEnumerable<T> enumerable, string separator = ",") 
			=> string.Join(separator, enumerable);
	}
}