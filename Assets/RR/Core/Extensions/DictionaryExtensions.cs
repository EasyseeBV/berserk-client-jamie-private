using System.Collections.Generic;
using System.Linq;

namespace RR.Core.Extensions
{
	public static class DictionaryExtensions
	{
		public static T GetValueOr<T>(this Dictionary<object, object> dictionary, string key, T otherwise)
		{
			dictionary.TryGetValue(key, out var value);

			if (value == null)
				return otherwise;

			return value is T castesValue ? castesValue : otherwise;
		}

		public static void ReplaceKey<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey fromKey, TKey toKey)
		{
			var value = dic[fromKey];
			dic.Remove(fromKey);
			dic[toKey] = value;
		}

		public static void SafeAddToCollectionValue<TKey, TItem>(this IDictionary<TKey, IEnumerable<TItem>> dic, TKey key, TItem item)
		{
			if (dic.TryGetValue(key, out var collection))
			{
				dic[key] = collection.Append(item);
				return;
			}

			dic.Add(key, new List<TItem> { item });
		}
	}
}