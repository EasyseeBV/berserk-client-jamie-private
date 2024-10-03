using System;
using System.Collections.Generic;
using System.Linq;

namespace RR.Core.Extensions
{
	public class TrackedOrderedDictionary<T, TK> : TrackedDictionary<T, TK>
	{
		public event Action<T, TK> OnModified;

		private List<KeyValuePair<T, TK>> _innerList = new List<KeyValuePair<T, TK>>();

		public new TK this[T key]
		{
			get => base[key];
			set => Set(key, value);
		}

		public override void Add(T key, TK value)
		{
			_innerList.Add(new KeyValuePair<T, TK>(key, value));
			base.Add(key, value);
		}

		public void Set(T key, TK value)
		{
			if (!TryGetValue(key, out var item))
				return;

			base[key] = value;
			var index = _innerList.IndexOf(_innerList.First(x => x.Key?.Equals(key) == true && x.Value?.Equals(item) == true));
			_innerList[index] = new KeyValuePair<T, TK>(key, value);
			OnModified?.Invoke(key, value);
		}

		public void Add(KeyValuePair<T, TK> value) => Add(value.Key, value.Value);
		public void AddRange(IEnumerable<KeyValuePair<T, TK>> set) => set.ForEach(Add);

		public override bool Remove(T key)
		{
			if (!TryGetValue(key, out var item))
				return false;

			_innerList.Remove(_innerList.First(x => x.Key.Equals(key) && x.Value.Equals(item)));
			return base.Remove(key);
		}

		public override void Clear()
		{
			_innerList.Clear();
			base.Clear();
		}

		public int LastIndexOf(TK item) => _innerList.LastIndexOf(_innerList.LastOrDefault(x => x.Value?.Equals(item) == true));
		public int LastIndexOf(T key) => _innerList.LastIndexOf(_innerList.LastOrDefault(x => x.Key?.Equals(key) == true));
	}
}