using System;
using System.Collections.Generic;
using System.Linq;

namespace RR.Core.Extensions
{
	public class TrackedList<T> : List<T>
	{
		public event Action<T> OnAdded;
		public event Action<T> OnRemoved;

		public new void Add(T item)
		{
			base.Add(item);
			OnAdded?.Invoke(item);
		}

		public void AddRangeDistinct(IEnumerable<T> set)
		{
			foreach (var item in set)
			{
				if (Contains(item))
					continue;

				Add(item);
			}
		}

		public new void AddRange(IEnumerable<T> set)
		{
			set.ForEach(Add);
		}

		public new void Remove(T text)
		{
			base.Remove(text);
			OnRemoved?.Invoke(text);
		}

		public new void Clear()
		{
			ForEach(x => OnRemoved?.Invoke(x));
			base.Clear();
		}

		public void RemoveDuplicates()
		{
			var dst = this.Distinct().ToHashSet();

			this.Except(dst).ForEach(x => OnRemoved?.Invoke(x));

			base.Clear();
			base.AddRange(dst);
		}
	}
}
