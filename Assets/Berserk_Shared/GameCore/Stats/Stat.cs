using System;
using Berserk.Shared.GameCore.LogicContext;
using Newtonsoft.Json;

namespace Berserk.Shared.GameCore
{
	[Serializable]
	public abstract class Stat<T> : IStat<T>
	{
		public event Action<T> OnChanged;
		public event Action<T, T> OnChangedFrom;

		[JsonProperty] public virtual string Name { get; set; }
		[JsonProperty] public virtual T Previous { get; protected set; }
		[JsonProperty] public virtual T Current { get; protected set; }
		[JsonProperty] public virtual T BaseStat { get; protected set; }
		[JsonProperty] public virtual T Max { get; protected set; }
		[JsonProperty] public StatChanges Changes { get; protected set; }
		[JsonIgnore] public virtual bool IsMax => Current.Equals(Max);
		[JsonIgnore] public virtual bool HasChanged { get; private set; }

		#region Constuctors

		[JsonConstructor]
		protected Stat(string name, T previous, T current, T baseStat, T max, StatChanges changes)
		{
			Name = name;
			Previous = previous;
			Current = current;
			BaseStat = baseStat;
			Max = max;
			Changes = changes;
		}
		
		protected Stat(Stat<T> stat)
		{
			if (stat == null)
				return;

			Replace(stat);
		}

		protected Stat(T max)
		{
			Previous = Current = BaseStat = Max = max;
		}

		protected Stat(T current, T max)
		{
			Max = max;
			BaseStat = Previous = Current = current;
		}

		#endregion

		#region Operators

		public static implicit operator T(Stat<T> stat) => stat.Current;

		#endregion

		protected virtual T ApplyCurrent(T value)
		{
			SetChanges(!Current.Equals(value), StatChanges.Current);
			Previous = Current;
			Current = value;
			return value;
		}

		protected void SetChanges(bool changes, StatChanges value)
		{
			HasChanged = HasChanged || changes;
			if (!changes || value == StatChanges.None) 
				return;
			
			if ((Changes & StatChanges.None) != 0)
				Changes ^= StatChanges.None;
			
			Changes |= value;
		}

		public IStat SetName(string value, bool notify = false)
		{
			SetChanges(Name != value, StatChanges.Name);
			Name = value;
			if (notify)
				NotifyChanges();
			return this;
		}

		public abstract void Add(T value, bool notify = true);
		public abstract void Substract(T value, bool notify = true);

		public abstract void MultiplyBy(T value, bool notify = true);

		public abstract void DivideBy(T value, bool notify = true);

		public IStat<T> Set(T value, bool notify = true)
		{
			ApplyCurrent(value);
			if (notify)
				NotifyChanges();

			return this;
		}

		public virtual IStat<T> Replace(IStat<T> other, bool notify = false)
		{
			if (other == null)
			{
				DefaultSharedLogger.Error("Other stat does not exist");
				return this;
			}
			
			SetChanges(!Previous.Equals(other.Previous), StatChanges.Previous);
			SetChanges(!Current.Equals(other.Current), StatChanges.Current);
			SetChanges(!Max.Equals(other.Max), StatChanges.Maximum);
			SetChanges(!BaseStat.Equals(other.BaseStat), StatChanges.Default);
			
			Previous = other.Previous;
			Current = other.Current;
			Max = other.Max;
			BaseStat = other.BaseStat;

			
			if (notify)
				NotifyChanges();

			return this;
		}

		public virtual IStat<T> SetAboveMax(T value, bool notify = true)
		{
			SetChanges(!Current.Equals(value), StatChanges.Current);
			Previous = Current;
			Current = value;
			
			if (notify)
				NotifyChanges();

			return this;
		}

		public virtual IStat<T> SetMax(T max, bool notify = false)
		{
			SetChanges(!Max.Equals(max), StatChanges.Maximum);
			Max = max;
			ApplyCurrent(Current); // apply maximum to current
			if (notify)
				NotifyChanges();

			return this;
		}

		public virtual void NotifyChanges(bool force = false)
		{
			if (!HasChanged && !force) 
				return;
			
			HasChanged = false;
			OnChanged?.Invoke(Current);
			OnChangedFrom?.Invoke(Previous, Current);
			Changes = StatChanges.None;
		}

		public virtual IStat<T> ResetToMax(bool notify = true)
		{
			return Set(Max, notify);
		}

		public override string ToString() => Current.ToString();

		public string ToString(bool includePrevious)
		{
			return includePrevious
				? $"{Name} : {Current}/{Max}, Default : {BaseStat}, Previous : {Previous}"
				: $"{Name} : {Current}/{Max}, Default : {BaseStat}";
		}

		public virtual void Dispose()
		{
			OnChanged = null;
			OnChangedFrom = null;
			HasChanged = false;
			
			Max = default;
			Name = default;
			Previous = default;
			Current  = default;
			BaseStat  = default;
			Changes  = default;
			HasChanged  = default;
		}
	}
}