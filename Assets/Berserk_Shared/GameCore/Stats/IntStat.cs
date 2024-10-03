using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.GameCore.LogicContext;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Berserk.Shared.GameCore
{
	[JsonConverter(typeof(StringEnumConverter)), Flags]
	public enum StatChanges
	{
		None = 2,
		Current = 4,
		Maximum = 8,
		Modifiers = 16,
		Previous = 32,
		Default = 64,
		Name = 128,
	}

	public class IntStat : Stat<int>, IStatModifiable<int>
	{
		#region Constructors

		[JsonConstructor]
		public IntStat(
			List<IStatModifier<int>> modifiers, 
			string name, 
			int previous, 
			int current, 
			int baseStat, 
			int max, 
			StatChanges changes) : base(name, previous, current, baseStat, max, changes)
		{
			Modifiers = modifiers;
			CalculateMidifiers();
		}

		public IntStat() : base(0)
		{
			CalculateMidifiers();
		}

		public IntStat(int max) : base(max)
		{
			CalculateMidifiers();
		}

		public IntStat(int current, int max) : base(current, max)
		{
			CalculateMidifiers();
		}

		public IntStat(IntStat copy) : base(copy)
		{
			CalculateMidifiers();
		}

		#endregion

		#region Override

		[JsonIgnore] public override bool IsMax => Current >= TotalMax;

		protected override int ApplyCurrent(int value)
		{
			value = value > TotalMax ? TotalMax : value;
			return value != Current
				? base.ApplyCurrent(value)
				: value;
		}

		public override IStat<int> Replace(IStat<int> other, bool notify = false)
		{
			if (other == null)
			{
				DefaultSharedLogger.Error("Other stat does not exist");
				return this;
			}

			if (other is IStatModifiable<int> modifiable)
			{
				// replace with new instances will be cloned to capture the state
				var clonedModifiers = modifiable.GetModifiers().Select(x => x.Clone()).ToArray();
				// it's not a very good comparison, but it should be.
				SetChanges(clonedModifiers.Length != Modifiers.Count || !notify, StatChanges.Modifiers);

				// when replace we don't need notify.
				Modifiers.Clear();
				Modifiers.AddRange(clonedModifiers);
			}

			base.Replace(other, notify);
			return this;
		}

		public override IStat<int> ResetToMax(bool notify = true)
		{
			return Set(TotalMax, notify);
		}

		public override void NotifyChanges(bool force = false)
		{
			CalculateMidifiers(false);
			base.NotifyChanges(force);
		}

		#endregion

		#region Modifiers

		[JsonProperty] protected List<IStatModifier<int>> Modifiers = new();
		[JsonIgnore] public int TotalMax => Max + ModMax;
		[JsonIgnore] public int ModMax { get; protected set; }

		// Encapsulation is for modifiers only, other changes will be incorrect.

		#region ModifierInternal

		void IStatModifiable<int>.AddModMax(int value)
		{
			ModMax += value;
		}

		void IStatModifiable<int>.SetModMax(int value)
		{
			ModMax = value;
		}

		#endregion

		public IEnumerable<IStatModifier<int>> GetModifiers()
		{
			return Modifiers.ToArray();
		}

		public IEnumerable<IStatModifier<int>> GetModifiers(string id)
		{
			return Modifiers.Where(x => x.Id == id).ToArray();
		}

		public void CalculateMidifiers(bool notify = true)
		{
			var modMaxPrevious = ModMax;
			ModMax = 0;
			var orderedModifiers = Modifiers.OrderByDescending(x => x.Priority).ToArray();
			// first calc all maximum
			foreach (var modifier in orderedModifiers)
				modifier.ApplyMaximum(this);
			// then calc all current, because current are depends to maximum
			foreach (var modifier in orderedModifiers)
				modifier.ApplyCurrent(this);

			ApplyCurrent(Current);
			SetChanges(modMaxPrevious != ModMax, StatChanges.Modifiers);
			
			if (notify)
				NotifyChanges();
		}

		public void AddModifier(IStatModifier<int> value, bool notify = true)
		{
			if (value == null)
			{
				DefaultSharedLogger.Error("Cannot apply null modifier");
				return;
			}

			if (Modifiers.Contains(value))
			{
				DefaultSharedLogger.Error("Cannot apply same modifier twice");
				return;
			}

			Modifiers.Add(value);
			SetChanges(true, StatChanges.Modifiers);

			if (notify)
				NotifyChanges();
		}

		public void RemoveModifier(IStatModifier<int> value, bool notify = true)
		{
			value?.Expire(this);
			SetChanges(Modifiers.Remove(value), StatChanges.Modifiers);
			
			if (Modifiers.Count == 0)
				ModMax = 0;
			
			if (!notify)
			{
				ApplyCurrent(Current);
				return;
			}
			
			NotifyChanges();
		}

		public void RemoveModifiers(string id, bool notify = true)
		{
			foreach (var modifier in Modifiers.OrderByDescending(x=> x.Priority).ToArray())
			{
				if (modifier?.Id == id)
					RemoveModifier(modifier, false);
			}

			if (notify)
				NotifyChanges();
		}

		public void ClearModifiers(bool notify = true)
		{
			SetChanges(Modifiers.Count > 0, StatChanges.Modifiers);
			ModMax = 0;
			Modifiers.Clear();

			if (notify)
				NotifyChanges();
		}

		#endregion

		public override void Add(int value, bool notify = true)
		{
			Set(Current + value, notify);
		}

		public override void Substract(int value, bool notify = true)
		{
			Set(Current - value, notify);
		}

		public override void MultiplyBy(int value, bool notify = true)
		{
			Set(Current * value, notify);
		}

		public override void DivideBy(int value, bool notify = true)
		{
			Set(Current / value, notify);
		}

		public void SetOrRaiseMax(int value, bool notify = true)
		{
			if (value > Max)
			{
				SetMax(value).ResetToMax(notify);
				return;
			}

			Set(value, notify);
		}

		public string ToString(string v) => Current.ToString(v);

		public override void Dispose()
		{
			base.Dispose();
			Modifiers.Clear();
			ModMax = default;
		}
	}
}