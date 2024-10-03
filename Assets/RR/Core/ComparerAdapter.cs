using System;
using System.Collections.Generic;

namespace RR.Core
{
    public class ComparerAdapter<T> : IComparer<T>
    {
        private readonly Comparison<T> comparison;

        public ComparerAdapter(Comparison<T> comparison)
        {
            this.comparison = comparison;
        }

        public int Compare(T x, T y)
        {
            return comparison.Invoke(x, y);
        }
    }
}