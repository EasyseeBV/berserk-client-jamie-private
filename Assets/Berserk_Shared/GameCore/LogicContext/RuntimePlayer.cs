using System;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Game;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.LogicContext
{
	public class RuntimePlayer : IRuntimePlayer
	{
		public event Action OnLavaChanged;
		public event Action OnHandChanged;
		public event Action OnDataChanged;
		public IRuntimePlayerData RuntimeData { get; private set; }

		public void Sync(IRuntimePlayerData runtimeData)
		{
			UnsubscribeRuntimeData();
			RuntimeData = runtimeData;
			RuntimeData.Mana.OnChanged += OnManaChanged;
			RuntimeData.HandCount.OnChanged += OnHandCountChanged;
		}

		public void Dispose()
		{
			OnDataChanged = null;
			OnLavaChanged = null;
			UnsubscribeRuntimeData();
		}

		public void SpendLava(int lava, bool notify = true)
		{
			if (lava < 0)
				throw new InvalidOperationException("Can't spend lava with negative value.");
			
			if (RuntimeData.Mana.IsMax)
			{
				// workaround for ManaGain effects when player has mana more than max
				var totalCurrent = RuntimeData.Mana - lava;
				RuntimeData.Mana.SetOrRaiseMax(totalCurrent, notify);
			}
			else
			{
				RuntimeData.Mana.Substract(lava, notify);
			}
		}

		public void SetReady(bool value, bool notify = true)
		{
			if (RuntimeData is IRuntimePlayerInternal dataInternal)
				dataInternal.IsReady = value;
			OnChangedNotify(notify);
		}

		public void SetFinishedMulligan(bool value, bool notify = true)
		{
			if (RuntimeData is IRuntimePlayerInternal dataInternal)
				dataInternal.IsFinishedMulligan = value;
			OnChangedNotify(notify);
		}

		public void SetLastRoundActive(int value, bool notify = true)
		{
			if (RuntimeData is IRuntimePlayerInternal dataInternal)
				dataInternal.LastRoundWithActive = value;
			OnChangedNotify(notify);
		}

		public void AddTurnWitoutCards(int value, bool notify = true)
		{
			if (RuntimeData is not IRuntimePlayerInternal dataInternal)
				return;
			
			dataInternal.TurnsWithoutCards ??= 0;
			dataInternal.TurnsWithoutCards += value;
			OnChangedNotify(notify);
		}

		public void AddPlayedCard(RuntimePlayedCardData value, bool notify = true)
		{
			RuntimeData.PlayedCards.Add(value);
			OnChangedNotify(notify);
		}

		public void RemovePlayerCardAt(int index, bool notify = true)
		{
			RuntimeData.PlayedCards.RemoveAt(index);
			OnChangedNotify(notify);
		}

		private void UnsubscribeRuntimeData()
		{
			if (RuntimeData?.Mana != null)
				RuntimeData.Mana.OnChanged -= OnManaChanged;
			
			if (RuntimeData?.HandCount != null)
				RuntimeData.HandCount.OnChanged -= OnHandCountChanged;
		}

		private void OnManaChanged(int value)
		{
			OnLavaChanged?.Invoke();
			OnChangedNotify(true);
		}
		
		private void OnHandCountChanged(int value)
		{
			OnHandChanged?.Invoke();
			OnChangedNotify(true);
		}

		private void OnChangedNotify(bool notify)
		{
			if (!notify)
				return;
			
			OnDataChanged?.Invoke();
		}
	}
}