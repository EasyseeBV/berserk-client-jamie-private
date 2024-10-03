using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Game.Effect_System;

namespace Vulcan.Data
{
	[Serializable]
	public class EffectsContainer
	{
		public List<EffectState> Values;

		public EffectsContainer()
		{
			Values = new List<EffectState>();
		}

		public EffectsContainer(IEnumerable<string> effectIds)
		{
			effectIds ??= Array.Empty<string>();
			Values = new List<EffectState>(effectIds.Select(id => new EffectState(id)));
		}

		public EffectsContainer(IEnumerable<EffectState> effects)
		{
			Values = effects?.ToList() ?? new List<EffectState>();
		}

		public List<EffectData> Effects => Values.Select(effectState => effectState.EffectData).ToList();

		public bool RequiredDelayDestruction => Effects.Any(effect =>
			effect.Phase == EffectPhase.BeforeDead || effect.Phase == EffectPhase.AfterDead || effect.Phase == EffectPhase.OnAfterDamaged);

		public bool Has(EffectKeyword effect)
		{
			return Values.Any(x => x.EffectData.Effect == effect);
		}

		public bool Has(EffectFamily family)
		{
			return Values.Any(x => x.EffectData.Effect.GetFamily() == family);
		}

		public EffectState GetOrNull(EffectKeyword effect)
		{
			return Values.FirstOrDefault(x => x.EffectData.Effect == effect);
		}

		public bool TryGet(EffectKeyword effect, out EffectState effectState)
		{
			effectState = Values.FirstOrDefault(x => x.EffectData.Effect == effect);
			return effectState != null;
		}

		public EffectState[] GetHeroAbilities()
		{
			return Values.Where(IsHeroAbility).ToArray();
		}
		public bool IsHeroAbility(EffectState value)
		{
			return value != null && value.Id.Contains("-HA");
		}

		public void ClearExept(params EffectState[] states)
		{
			Values.Clear();
			Values.AddRange(states.Where(x=> x != null));
		}

		// New effect with unique UID with redefinition of length and value;
		public string Add(EffectKeyword effect, int? length = null, int? value = null, params string[] args)
		{
			var state = new EffectState(effect, length, value, args);
			Values.Add(state);
			return state.UID;
		}

		// New effect with unique UID with redefinition of length and value;
		public string AddFirst(EffectKeyword effect, int? length = null, int? value = null, params string[] args)
		{
			var state = new EffectState(effect, length, value, args);
			Values.Insert(0, state);
			return state.UID;
		}

		// New effect with unique UID with redefinition of length and value;
		public string Add(EffectData effectData, params string[] args)
		{
			var state = new EffectState(effectData, args);
			Values.Add(state);
			return state.UID;
		}

		// New effect with unique UID with redefinition of length and value;
		public string AddFirst(EffectData effectData, params string[] args)
		{
			var state = new EffectState(effectData, args);
			Values.Insert(0, state);
			return state.UID;
		}

		/// <summary>
		///     Decrease length by 1. Do not use in capture loops like foreach.
		/// </summary>
		/// <param name="data"></param>
		public bool StepForward(EffectData data)
		{
			var state = Values.FirstOrDefault(x => x.EffectData.Equals(data));
			if (state == null || state.Length < 0)
				return false;

			state.Length = Math.Max(0, state.Length - 1); //effects with -1 length are infinite

			return state.Length >= 0;
		}

		public void Remove(EffectState state)
		{
			Values.Remove(state);
		}

		public void Remove(string effectStateId)
		{
			var state = Values.FirstOrDefault(x => x.UID == effectStateId);
			Values.Remove(state);
		}

		public void Remove(EffectKeyword effect)
		{
			var state = Values.FirstOrDefault(x => x.EffectData.Effect == effect);
			if (state == null)
				return;
			Remove(state);
		}

		public override string ToString()
		{
			return string.Join("\n", Values.Select(x => $"{x}"));
		}
	}
}