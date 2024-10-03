using System;
using System.Collections.Generic;

namespace RR.Core
{
    public class EqualityComparerAdapter<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> comparer;
        private readonly Func<T, int> hashcode;

        public EqualityComparerAdapter(Func<T, T, bool> comparer, Func<T, int> hashcode = null)
        {
            this.comparer = comparer;
            this.hashcode = hashcode;
        }

        public bool Equals(T x, T y)
        {
            return comparer.Invoke(x, y);
        }

        public int GetHashCode(T obj)
        {
            return hashcode?.Invoke(obj) ?? 0;
        }
    }
}