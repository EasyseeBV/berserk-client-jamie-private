using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.RuntimeObjects
{
	public class RuntimeGameCard : RuntimeGameObject, IRuntimeGameCard
	{
		public new IRuntimeCardData RuntimeData => (IRuntimeCardData) base.RuntimeData;
		public new ICardData Data => (ICardData) base.Data;

		public override bool IsDead => RuntimeData.State is RuntimeState.InDiscard or RuntimeState.InExile
		                               || RuntimeData.Hp <= 0;

		protected override void OnDied()
		{
			ReturnToDiscard();
		}

		public override void Spawn(bool notify = true)
		{
			base.Spawn(notify);
			RuntimeData.MoveCount.ResetToMax();
		}

		public void ReturnToDeck(bool notify = true)
		{
			RuntimeData.ResetRelativePositionX();
			UpdateState(RuntimeState.InDeck, notify);
		}

		public void ReturnToHand(bool notify = true)
		{
			RuntimeData.ResetRelativePositionX();
			UpdateState(RuntimeState.InHand, notify);
		}

		public virtual void ReturnToTable(bool notify = true)
		{
			// Important don't reset here the position to default, use it directly where you need.
			UpdateState(RuntimeState.InTable, notify);
		}

		public void ReturnToChoose(bool notify = true)
		{
			// Important don't reset here the position to default, use it directly where you need.
			UpdateState(RuntimeState.InChoose, notify);
		}

		public void ReturnToShowAll(bool notify = true)
		{
			RuntimeData.ResetRelativePositionX();
			UpdateState(RuntimeState.InShowAll, notify);
		}

		public void ReturnToShow(bool notify = true)
		{
			RuntimeData.ResetRelativePositionX();
			UpdateState(RuntimeState.InShow, notify);
		}

		public void TurnToken(bool value)
		{
			if(value)
				RuntimeData.Mana.SetMax(0);
			
			RuntimeData.TurnToken(value);
		}

		public void ReturnToDiscard(bool notify = true)
		{
			if (RuntimeData.IsToken)
			{
				ReturnToExile(notify);
				return;
			}

			RuntimeData.ResetRelativePositionX();
			UpdateState(RuntimeState.InDiscard, notify);
			ResetEffects();
			ResetStatsToDefault();
		}

		public void ReturnToExile(bool notify = true)
		{
			RuntimeData.ResetRelativePositionX();
			UpdateState(RuntimeState.InExile, notify);
			ResetEffects();
			ResetStatsToDefault();
		}

		public void UpdateState(RuntimeState current, bool notify = true)
		{
			UpdateState(current, null, notify);
		}

		public void UpdateState(RuntimeState current, RuntimeState? prev, bool notify = true)
		{
			if (notify)
				RuntimeData.SetState(current, prev);
			else
				RuntimeData.SetStateWithoutNotify(current, prev);
		}
	}
}