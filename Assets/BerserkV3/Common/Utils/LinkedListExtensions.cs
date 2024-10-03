using System.Collections.Generic;
using System.Linq;

namespace BerserkV3.Common.Utils
{
	public static class LinkedListExtensions
	{
		public static void Push<T>(this LinkedList<T> list, T obj)
		{
			list.AddFirst(obj);
		}
		
		public static bool TryPop<T>(this LinkedList<T> list, out T obj)
		{
			if (list.Count == 0)
			{
				obj = default;
				return false;
			}

			obj = list.Pop();
			return true;
		}
		
		public static T Pop<T>(this LinkedList<T> list)
		{
			T first = list.First();
			list.RemoveFirst();
			return first;
		}
	}
}