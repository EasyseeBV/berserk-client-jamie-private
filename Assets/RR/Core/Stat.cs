using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RR.Core
{
	[Serializable]
	public class DoubleStat : Stat<double>, IComparable<DoubleStat>, IComparable<Stat<double>>, IComparable<double>, IEquatable<DoubleStat>
	{
		public override bool IsMax => Math.Abs(Max - Current) < double.Epsilon;

		#region Constructors

		[JsonConstructor] public DoubleStat(object max) : base(max) { }
		public DoubleStat(DoubleStat copy) : base(copy) { }
		public DoubleStat() : base(default) { }
		public DoubleStat(double max) : base(max) { }
		public DoubleStat(double current, double max) : base(current, max) { }
		public DoubleStat(double current, double @default, double max) : base(current, @default, max) { }

		#endregion

		#region Operators

		public static DoubleStat operator ++(DoubleStat stat)
		{
			stat.Set(stat.Current + 1);
			return stat;
		}

		public static DoubleStat operator --(DoubleStat stat)
		{
			stat.Set(stat.Current - 1);
			return stat;
		}

		/// <summary>
		/// Beware of new() memory usage when doing routine operations.
		/// </summary>
		public static DoubleStat operator +(DoubleStat stat, DoubleStat value)
		{
			var result = stat.Current + value.Current;
			var newStat = new DoubleStat(result, result);
			return newStat;
		}

		/// <summary>
		/// Beware of new() memory usage when doing routine operations.
		/// </summary>
		public static DoubleStat operator -(DoubleStat stat, DoubleStat value)
		{
			var result = stat.Current - value.Current;
			var newStat = new DoubleStat(result);
			return newStat;
		}

		/// <summary>
		/// Beware of new() memory usage when doing routine operations.
		/// </summary>
		public static DoubleStat operator *(DoubleStat stat, DoubleStat value)
		{
			var result = stat.Current * value.Current;
			var newStat = new DoubleStat(result);
			return newStat;
		}

		/// <summary>
		/// Beware of new() memory usage when doing routine operations.
		/// </summary>
		public static DoubleStat operator /(DoubleStat stat, DoubleStat value)
		{
			var result = stat.Current / value.Current;
			var newStat = new DoubleStat(result);
			return newStat;
		}

		public static double operator +(float value, DoubleStat stat) => value + stat.Current;
		public static double operator -(float value, DoubleStat stat) => value - stat.Current;
		public static double operator *(float value, DoubleStat stat) => value * stat.Current;
		public static double operator /(float value, DoubleStat stat) => value / stat.Current;
		public static double operator +(DoubleStat stat, float value) => stat.Current + value;
		public static double operator -(DoubleStat stat, float value) => stat.Current - value;
		public static double operator *(DoubleStat stat, float value) => stat.Current * value;
		public static double operator /(DoubleStat stat, float value) => stat.Current / value;

		public static bool operator <(DoubleStat left, double right) => right.CompareTo(left) > 0;
		public static bool operator >(DoubleStat left, double right) => right.CompareTo(left) < 0;
		public static bool operator ==(DoubleStat left, double right)
		{
			if (left is null) return false;
			return left.Current.Equals(right);
		}
		public static bool operator !=(DoubleStat left, double right)
		{
			if (left is null) return true;
			return !left.Current.Equals(right);
		}

		public static bool operator <(double left, DoubleStat right) => right.CompareTo(left) > 0;
		public static bool operator >(double left, DoubleStat right) => right.CompareTo(left) < 0;
		public static bool operator ==(double left, DoubleStat right)
		{
			if (right is null) return false;
			return right.Current.Equals(left);
		}
		public static bool operator !=(double left, DoubleStat right)
		{
			if (right is null) return true;
			return !right.Current.Equals(left);
		}

		public static bool operator <(DoubleStat left, DoubleStat right) => right.CompareTo(left) > 0;
		public static bool operator >(DoubleStat left, DoubleStat right) => right.CompareTo(left) < 0;
		public static bool operator ==(DoubleStat left, DoubleStat right)
		{
			if (right is null) return left is null;
			return right.Current.Equals(left);
		}
		public static bool operator !=(DoubleStat left, DoubleStat right)
		{
			if (right is null) return left is null;
			return !right.Current.Equals(left);
		}

		public static implicit operator double(DoubleStat stat) => stat?.Current ?? 0;
		public static explicit operator DoubleStat(double stat) => new DoubleStat(stat);
		public static explicit operator DoubleStat(float stat) => new DoubleStat(stat);

		#endregion

		#region Override

		protected override void ApplyCurrent(double value)
		{
			Previous = Current;
			Current = value > Max ? Max : value;
		}

		#endregion

		#region Equatable

		public override int GetHashCode()
			=> base.GetHashCode();

		public bool Equals(DoubleStat other)
			=> other != null && Math.Abs(other.Current - Current) < double.Epsilon;

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((DoubleStat)obj);
		}

		#endregion

		public void Add(double value) => Set(Current + value);
		public void MultiplyBy(double value) => Set(Current * value);
		public void DivideBy(double value) => Set(Current / value);

		public double PercentOfMax() => Max != 0 ? Current / Max : 1;
		public string ToString(string v) => Current.ToString(v);
		public override string ToString() => Current.ToString("####");

		public int CompareTo(DoubleStat other) => Current.CompareTo(other.Current);
		public int CompareTo(Stat<double> other) => Current.CompareTo(other);
		public int CompareTo(double other) => Current.CompareTo(other);
	}

	[Serializable]
	public class FloatStat : Stat<float>, IComparable<FloatStat>, IComparable<Stat<float>>, IComparable<float>
	{
		public override bool IsMax => Math.Abs(Max - Current) < float.Epsilon;

		#region Constructors

		[JsonConstructor] public FloatStat(object max) : base(max) { }
		public FloatStat(FloatStat copy) : base(copy) { }
		public FloatStat() : base(default) { }
		public FloatStat(float max) : base(max) { }
		public FloatStat(float current, float max) : base(current, max) { }
		public FloatStat(float current, float @default, float max) : base(current, @default, max) { }

		#endregion

		#region Operators

		public static FloatStat operator ++(FloatStat stat)
		{
			stat.Set(stat.Current + 1);
			return stat;
		}

		public static FloatStat operator --(FloatStat stat)
		{
			stat.Set(stat.Current - 1);
			return stat;
		}

		/// <summary>
		/// Beware of new() memory usage when doing routine operations.
		/// </summary>
		public static FloatStat operator +(FloatStat stat, FloatStat value)
		{
			var result = stat.Current + value.Current;
			var newStat = new FloatStat(result, result);
			return newStat;
		}

		/// <summary>
		/// Beware of new() memory usage when doing routine operations.
		/// </summary>
		public static FloatStat operator -(FloatStat stat, FloatStat value)
		{
			var result = stat.Current - value.Current;
			var newStat = new FloatStat(result);
			return newStat;
		}

		/// <summary>
		/// Beware of new() memory usage when doing routine operations.
		/// </summary>
		public static FloatStat operator *(FloatStat stat, FloatStat value)
		{
			var result = stat.Current * value.Current;
			var newStat = new FloatStat(result);
			return newStat;
		}

		/// <summary>
		/// Beware of new() memory usage when doing routine operations.
		/// </summary>
		public static FloatStat operator /(FloatStat stat, FloatStat value)
		{
			var result = stat.Current / value.Current;
			var newStat = new FloatStat(result);
			return newStat;
		}

		public static float operator +(float value, FloatStat stat) => value + stat.Current;
		public static float operator -(float value, FloatStat stat) => value - stat.Current;
		public static float operator *(float value, FloatStat stat) => value * stat.Current;
		public static float operator /(float value, FloatStat stat) => value / stat.Current;
		public static float operator +(FloatStat stat, float value) => stat.Current + value;
		public static float operator -(FloatStat stat, float value) => stat.Current - value;
		public static float operator *(FloatStat stat, float value) => stat.Current * value;
		public static float operator /(FloatStat stat, float value) => stat.Current / value;

		public static bool operator <(FloatStat left, float right) => right.CompareTo(left) > 0;
		public static bool operator >(FloatStat left, float right) => right.CompareTo(left) < 0;
		public static bool operator ==(FloatStat left, float right)
		{
			if (left is null) return false;
			return left.Current.Equals(right);
		}
		public static bool operator !=(FloatStat left, float right)
		{
			if (left is null) return true;
			return !left.Current.Equals(right);
		}

		public static bool operator <(float left, FloatStat right) => right.CompareTo(left) > 0;
		public static bool operator >(float left, FloatStat right) => right.CompareTo(left) < 0;
		public static bool operator ==(float left, FloatStat right)
		{
			if (right is null) return false;
			return right.Current.Equals(left);
		}
		public static bool operator !=(float left, FloatStat right)
		{
			if (right is null) return true;
			return !right.Current.Equals(left);
		}

		public static bool operator <(FloatStat left, FloatStat right) => right.CompareTo(left) > 0;
		public static bool operator >(FloatStat left, FloatStat right) => right.CompareTo(left) < 0;
		public static bool operator ==(FloatStat left, FloatStat right)
		{
			if (right is null) return left is null;
			return right.Current.Equals(left);
		}
		public static bool operator !=(FloatStat left, FloatStat right)
		{
			if (right is null) return left is null;
			return !right.Current.Equals(left);
		}

		public static implicit operator float(FloatStat stat) => stat?.Current ?? 0;
		public static explicit operator FloatStat(float stat) => new FloatStat(stat);
		public static explicit operator FloatStat(double stat) => new FloatStat(stat);

		#endregion

		#region Override

		protected override void ApplyCurrent(float value)
		{
			Previous = Current;
			Current = value > Max ? Max : value;
		}

		#endregion

		#region Equatable

		public override int GetHashCode()
			=> base.GetHashCode();

		public bool Equals(FloatStat other)
			=> other != null && Mathf.Approximately(other.Current, Current);

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((FloatStat)obj);
		}

		#endregion

		public void Add(float value) => Set(Current + value);
		public void MultiplyBy(float value) => Set(Current * value);
		public void DivideBy(float value) => Set(Current / value);

		public float PercentOfMax() => Max != 0 ? Current / Max : 1;
		public string ToString(string v) => Current.ToString(v);

		public int CompareTo(FloatStat other) => Current.CompareTo(other.Current);
		public int CompareTo(Stat<float> other) => Current.CompareTo(other);
		public int CompareTo(float other) => Current.CompareTo(other);
	}

	public class IntStat : Stat<int>, IComparable<Stat<int>>, IComparable<IntStat>, IComparable<int>
	{
		#region Constructors

		[JsonConstructor] public IntStat(object max) : base(max) { }
		public IntStat() : base(default) { }
		public IntStat(int max) : base(max) { }
		public IntStat(int current, int max) : base(current, max) { }
		public IntStat(IntStat copy) : base(copy) { }

		#endregion

		#region Operators

		public static IntStat operator ++(IntStat stat)
		{
			stat.Set(stat.Current + 1);
			return stat;
		}

		public static IntStat operator --(IntStat stat)
		{
			stat.Set(stat.Current - 1);
			return stat;
		}

		/// <summary>
		/// Beware of new() memory usage when doing routine operations.
		/// </summary>
		public static IntStat operator +(IntStat stat, IntStat value)
		{
			var result = stat.Current + value.Current;
			var newStat = new IntStat(result, result);
			return newStat;
		}

		/// <summary>
		/// Beware of new() memory usage when doing routine operations.
		/// </summary>
		public static IntStat operator -(IntStat stat, IntStat value)
		{
			var result = stat.Current - value.Current;
			var newStat = new IntStat(result);
			return newStat;
		}

		/// <summary>
		/// Beware of new() memory usage when doing routine operations.
		/// </summary>
		public static IntStat operator *(IntStat stat, IntStat value)
		{
			var result = stat.Current * value.Current;
			var newStat = new IntStat(result);
			return newStat;
		}

		/// <summary>
		/// Beware of new() memory usage when doing routine operations.
		/// </summary>
		public static IntStat operator /(IntStat stat, IntStat value)
		{
			var result = stat.Current / value.Current;
			var newStat = new IntStat(result);
			return newStat;
		}

		public static int operator +(IntStat stat, int value) => stat.Current + value;
		public static int operator -(IntStat stat, int value) => stat.Current - value;
		public static int operator *(IntStat stat, int value) => stat.Current * value;
		public static int operator /(IntStat stat, int value) => stat.Current / value;

		public static int operator +(int value, IntStat stat) => value + stat.Current;
		public static int operator -(int value, IntStat stat) => value - stat.Current;
		public static int operator *(int value, IntStat stat) => value * stat.Current;
		public static int operator /(int value, IntStat stat) => value / stat.Current;

		public static bool operator <(IntStat left, int right) => right.CompareTo(left) > 0;
		public static bool operator >(IntStat left, int right) => right.CompareTo(left) < 0;
		public static bool operator ==(IntStat left, int right)
		{
			if (left is null) return false;
			return left.Current.Equals(right);
		}
		public static bool operator !=(IntStat left, int right)
		{
			if (left is null) return true;
			return !left.Current.Equals(right);
		}

		public static bool operator <(int left, IntStat right) => right.CompareTo(left) > 0;
		public static bool operator >(int left, IntStat right) => right.CompareTo(left) < 0;
		public static bool operator ==(int left, IntStat right)
		{
			if (right is null) return false;
			return right.Current.Equals(left);
		}
		public static bool operator !=(int left, IntStat right)
		{
			if (right is null) return true;
			return !right.Current.Equals(left);
		}

		public static bool operator <(IntStat left, IntStat right) => right.CompareTo(left) > 0;
		public static bool operator >(IntStat left, IntStat right) => right.CompareTo(left) < 0;
		public static bool operator ==(IntStat left, IntStat right)
		{
			if (right is null) return left is null;
			return right.Current.Equals(left);
		}
		public static bool operator !=(IntStat left, IntStat right)
		{
			if (right is null) return left is null;
			return !right.Current.Equals(left);
		}

		public static implicit operator int(IntStat stat) => stat?.Current ?? 0;
		public static explicit operator IntStat(int stat) => new IntStat(stat);

		#endregion

		#region Override

		protected override void ApplyCurrent(int value)
		{
			Previous = Current;
			Current = value > Max ? Max : value;
		}

		#endregion

		#region Equatable

		public override int GetHashCode()
			=> base.GetHashCode();

		public bool Equals(IntStat other)
			=> other != null && other.Current == Current;

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((IntStat)obj);
		}

		#endregion

		public void Add(int value) => Set(Current + value);
		public void MultiplyBy(int value) => Set(Current * value);
		public void DivideBy(int value) => Set(Current / value);


		public void SetOrRaiseMax(int value)
		{
			if (value > Max) SetMax(value, true);
			else Set(value);
		}

		public string ToString(string v) => Current.ToString(v);

		public int CompareTo(IntStat other) => Current.CompareTo(other.Current);

		public int CompareTo(Stat<int> other) => Current.CompareTo(other);
		public int CompareTo(int other) => Current.CompareTo(other);
	}

	public interface IStat
	{

	}

	[Serializable]
	public class Stat<T> : IStat, IEquatable<Stat<T>>
	{
		public event Action<T> OnChanged;
		public event Action<T, T> OnChangedFrom;

		[SerializeField, JsonProperty] protected T Max;
		[SerializeField, JsonProperty] protected T Current;
		[SerializeField, JsonProperty] protected T Default;

		[JsonIgnore] protected T Previous;
		[JsonIgnore] public virtual bool IsMax => Current.Equals(Max);

		#region Constuctors

		public Stat(Stat<T> stat)
		{
			if (stat == null)
				return;

			Previous = stat.Previous;
			Current = stat.Current;
			Default = stat.Default;
			Max = stat.Max;
		}

		[JsonConstructor]
		public Stat(object max)
		{
			if (max is T maxIsT)
			{
				Previous = Current = Default = Max = maxIsT;
				return;
			}

			Previous = Current = Default = Max = (T)Convert.ChangeType(max, typeof(T));
		}

		public Stat(T max)
		{
			Previous = Current = Default = Max = max;
		}

		/// <summary>
		/// Sets <see cref="Default"/> to <see cref="Max"/> (<see cref="maxAndDefault"/>
		/// </summary>
		public Stat(T current, T max)
		{
			Max = max;
			Default = Previous = Current = current;
		}

		[JsonConstructor]
		public Stat(T current, T @default, T max)
		{
			Max = max;
			Previous = Current = current;
			Default = @default;
		}

		#endregion

		#region Getters

		public T GetMax() => Max;
		public T GetDefault() => Default;

		#endregion

		#region Equatable

		public bool Equals(Stat<T> other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return EqualityComparer<T>.Default.Equals(Current, other.Current);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((Stat<T>)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = EqualityComparer<T>.Default.GetHashCode(Max);
				hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(Current);
				hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(Default);
				return hashCode;
			}
		}

		public static bool operator ==(Stat<T> left, Stat<T> right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Stat<T> left, Stat<T> right)
		{
			return !Equals(left, right);
		}

		#endregion

		protected virtual void ApplyCurrent(T value)
		{
			Previous = Current;
			Current = value;
		}

		/// <summary>
		/// Sets Current value
		/// </summary>
		public void Set(T value)
		{
			ApplyCurrent(value);
			OnChanged?.Invoke(Current);
			OnChangedFrom?.Invoke(Previous, Current);
		}

		/// <summary>
		/// Sets <see cref="Current"/> value above <see cref="Max"/>
		/// </summary>
		public void SetAboveMax(T value)
		{
			Previous = Current;
			Current = value;
			OnChanged?.Invoke(Current);
			OnChangedFrom?.Invoke(Previous, Current);
		}
		/// <summary>
		/// Sets Current value to <see cref="Default"/>
		/// </summary>
		public void ResetCurrentToDefault() => Set(Default);

		public void ResetMaxToDefault()
		{
			Max = Default;
			ApplyCurrent(Current);
		}

		public void SetMax(T max, bool resetToMax)
		{
			SetMax(max);
			if (resetToMax) ResetToMax();
		}

		public void SetMax(T max)
		{
			Max = max;
			ApplyCurrent(Current);
		}

		/// <summary>
		/// Sets Current value to <see cref="Max"/>
		/// </summary>
		public void ResetToMax() => Set(Max);

		/// <summary>
		/// Sets Current value to <see cref="Default"/>
		/// </summary>
		public void ResetToDefault() => Set(Default);

		/// <summary>
		/// Sets Current value to <see cref="default{T}"/>
		/// </summary>
		public void ResetClear() => Set(default);

		public static implicit operator T(Stat<T> stat) => stat.Current;
		public static explicit operator Stat<T>(T stat) => new Stat<T>(stat);

		public override string ToString()
			=> Current.ToString();

		public string ToString(bool includePrevious)
			=> includePrevious
				? $"{Current}/{Max} ({Default}) (Prev: {Previous})"
				: $"{Current}/{Max} ({Default})";
	}
}
