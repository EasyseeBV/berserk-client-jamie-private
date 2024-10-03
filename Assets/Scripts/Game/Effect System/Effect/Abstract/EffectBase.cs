using System;
using Berserk.Shared.Data.Enums;
using Events;
using Game.Entities;
using UnityEngine;
using Vulcan.Data;
using EffectData = Vulcan.Data.EffectData;
using EffectValue = Vulcan.Data.EffectValue;
using Owner = Berserk.Shared.Data.Enums.Owner;

namespace Game.Effect_System.Effects
{
	public abstract class EffectBase : IEffectBehaviour
	{
		protected PickInfo PickInfo;
		protected EffectData Data => PickInfo.EffectState.EffectData;
		protected MonoBehaviour MonoBeh => GameBus.LocalContext.GetVulcaniteByOwner(Owner.Self);

		public void Fill(PickInfo pickInfo)
		{
			PickInfo = pickInfo;
		}

		/// <summary>
		///     Decrease effect lenght by 1. Cancel and remove effect if it's lenght expired.
		/// </summary>
		/// <returns>True if effect has been canceled and removed</returns>
		public virtual bool StepForward()
		{
			var effectContainer = PickInfo?.From?.DataBase?.EffectsContainer;
			if (effectContainer != null && effectContainer.StepForward(Data))
			{
				var state = PickInfo.EffectState;
				if (state.Length == 0 && !effectContainer.IsHeroAbility(state))
				{
					Cancel();
					effectContainer.Remove(state);
				}

				return true;
			}

			return false;
		}

		public virtual void Cancel()
		{
		}

		public abstract void Perform(IInteractiveEntity[] targets);

		/// <summary>
		///     Add effect from pickInfo.From Entity. If effect not specified, add current.
		/// </summary>
		protected string AddOwnerEffect(EffectKeyword effect = 0,
			int length = int.MinValue,
			int value = int.MinValue,
			params string[] args)
		{
			if (effect == 0)
			{
				var effectAttribute = (EffectAttribute)Attribute.GetCustomAttribute(GetType(), typeof(EffectAttribute));
				effect = effectAttribute.Effect;
			}

			return PickInfo.From.DataBase.EffectsContainer.Add(effect, length, value, args);
		}

		protected string AddOwnerEffect(EffectData data, params string[] args)
		{
			return PickInfo.From.DataBase.EffectsContainer.Add(data, args);
		}

		protected void RemoveOwnerEffect(string id = null)
		{
			if (string.IsNullOrEmpty(id))
			{
				var effectAttribute = (EffectAttribute)Attribute.GetCustomAttribute(GetType(), typeof(EffectAttribute));
				var effect = effectAttribute.Effect;
				PickInfo.From.DataBase.EffectsContainer.Remove(effect);
				return;
			}

			PickInfo.From.DataBase.EffectsContainer.Remove(id);
		}

		protected int ValueMod(CardStat cardStat)
		{
			return Data.ValueMod == EffectValue.Default
				? Data.Value //Integer Value
				: (int)Mathf.Floor(cardStat * Data.Value / 100f);
			//Nearest integer value From Percent
		}

		protected int ValueModCeil(CardStat cardStat)
		{
			return Data.ValueMod == EffectValue.Default
				? Data.Value //Integer Value
				: (int)Mathf.Ceil(cardStat * Data.Value / 100f);
			//Nearest integer value From Percent
		}

		protected int ValueModMaximum(CardStat cardStat)
		{
			return Data.ValueMod == EffectValue.Default
				? Data.Value //Integer Value
				: (int)Mathf.Floor(cardStat.GetMax() * Data.Value / 100f);
			//Nearest integer value From base Percent 
		}
	}
}