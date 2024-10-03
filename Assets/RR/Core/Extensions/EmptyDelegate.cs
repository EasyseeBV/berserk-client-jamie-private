using System;

namespace RR.Core.Extensions
{
	public static class EmptyDelegate
	{
		public static readonly Action Action = () => { };
	}

	public static class EmptyDelegate<T>
	{
		public static readonly Action<T> Action = x => { };
	}

	public static class EmptyDelegate<T, TK>
	{
		public static readonly Action<T, TK> Action = (x, y) => { };
	}
}
