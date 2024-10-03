using System;

namespace Berserk.Shared.GameCore
{
	public interface IStat : IDisposable
	{
		bool IsMax { get; }
		string Name { get; set; }
		void NotifyChanges(bool force = false);
		IStat SetName(string value, bool notify = false);
	}

	public interface IStat<T> : IStat
	{
		event Action<T> OnChanged;
		event Action<T, T> OnChangedFrom;
		
		T Previous { get; }
		T Current { get; }
		T BaseStat { get; }
		T Max { get; }

		void Add(T value, bool notify = true);
		void MultiplyBy(T value, bool notify = true);
		void DivideBy(T value, bool notify = true);
		
		IStat<T> Set(T value, bool notify = true);
		IStat<T> Replace(IStat<T> other, bool notify = false);
		IStat<T> SetAboveMax(T value, bool notify = true);
		IStat<T> SetMax(T max, bool notify = false);
		IStat<T> ResetToMax(bool notify = true);
	}
}