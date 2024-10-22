using System;
using Newtonsoft.Json;

namespace Berserk.Shared.GameCore
{
	public class SimpleModifierInt : IStatModifier<int>
	{
		[JsonProperty] public string Id { get; protected set; }
		[JsonProperty] public int Priority { get; protected set; }
		[JsonProperty] public int ModifierCurrent { get; protected set; }
		[JsonProperty] public int ModifierMaximum { get; protected set; }
		[JsonProperty] public int MaximumApplied { get; protected set; }
		[JsonProperty] public int CurrentApplied { get; protected set; }
		[JsonProperty] protected bool RemoveMaxWhenExpire;
		[JsonProperty] protected bool RemoveCurrWhenExpire;
		
		#region Setters

		public IStatModifier<int> SetModifierId(string id)
		{
			Id = id;
			return this;
		}

		public IStatModifier<int> SetPriority(int value)
		{
			Priority = value;
			return this;
		}

		public IStatModifier<int> SetMaxModifier(int value, int applied = 0, bool revertWhenExpire = true)
		{
			ModifierMaximum = value;
			RemoveMaxWhenExpire = revertWhenExpire;
			MaximumApplied = applied;
			return this;
		}

		public IStatModifier<int> SetCurrModifier(int value, int applied = 0, bool revertWhenExpire = false)
		{
			ModifierCurrent = value;
			RemoveCurrWhenExpire = revertWhenExpire;
			CurrentApplied = applied;
			return this;
		}

		public ModifierStack<int> GetDataStack(ModifierStack<int> other = default)
		{
			other.ModifierCurrentTotal += ModifierCurrent;
			other.ModifierMaximumTotal += ModifierMaximum;
			other.CurrentAppliedTotal += CurrentApplied;
			other.MaximumAppliedTotal += MaximumApplied;
			return other;
		}

		#endregion

		public virtual void Apply(IStatModifiable<int> stat)
		{
			MaximumApplied = Apply(stat.AddModMax, ModifierMaximum);
			CurrentApplied = Apply(stat.AddModCurrent, ModifierCurrent);
		}

		public virtual void Expire(IStatModifiable<int> stat)
		{
			if (ModifierMaximum != 0 && RemoveMaxWhenExpire)
			{
				Apply(stat.AddModMax, Sign(MaximumApplied, true));
				MaximumApplied = 0;
			}

			if (CurrentApplied != 0 && RemoveCurrWhenExpire)
			{
				Apply(stat.AddModCurrent, Sign(CurrentApplied, true));
				CurrentApplied = 0;
			}
		}

		protected virtual int Apply(Action<int> applyfunc, int? value)
		{
			if (value is null or 0) 
				return 0;
			
			applyfunc?.Invoke(value.Value);
			return value.Value;
		}
		
		protected virtual int Sign(int? value, bool remove = false)
		{
			return (value ?? 0) * (remove ? -1 : 1);
		}
	}
}