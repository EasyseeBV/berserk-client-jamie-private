using System;
using System.Collections.Generic;
using System.Linq;

namespace RR.Core.Extensions
{
	public class TrackedDictionary<T, TK> : Dictionary<T, TK>
	{
		public event Action<T, TK> OnAdded;
		public event Action<T, TK> OnRemoved;

		public new virtual void Add(T key, TK value)
		{
			base.Add(key, value);
			OnAdded?.Invoke(key, value);
		}

		public new virtual bool Remove(T key)
		{
			if (!TryGetValue(key, out var item))
				return false;

			base.Remove(key);
			OnRemoved?.Invoke(key, item);
			return true;
		}
		
		public new virtual void Clear()
		{
			var itemsToClear = this.ToList();
			base.Clear();
			itemsToClear.ForEach(x => OnRemoved?.Invoke(x.Key, x.Value));
		}
	}
}