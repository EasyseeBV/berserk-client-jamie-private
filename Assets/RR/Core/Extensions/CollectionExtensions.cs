using System.Collections.Generic;

namespace RR.Core.Extensions
{
	public static class CollectionExtensions
	{
		public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
		{
			items.ForEach(collection.Add);
		}
	}
}