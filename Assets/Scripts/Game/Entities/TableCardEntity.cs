using Berserk.Shared.Data.Enums;
using Events;
using UI;
using UnityEngine;
using Vulcan.Network;
using CardData = Vulcan.Data.CardData;

namespace Game.Entities
{
	public abstract class TableCardEntity : InteractiveEntity<CardData>
	{
		protected new TableCardView View => (TableCardView)base.View;

		protected override void Awake()
		{
			base.Awake();

			GameBus.OnSpawnConfirmed.SubscribeFirst(this, x => x.UID.Equals(Data.UID) && !x.IsSpawnedByEffect, ConfirmSpawnCard);
			GameBus.OnSpawnCanceled.Subscribe(this, x => x.UID.Equals(Data.UID), CancelSpawnCard);
		}

		public void SetPosition(Vector3 position)
		{
			View.SetPosition(position, Quaternion.identity);
		}

		protected abstract void AddToContext();

		protected abstract void RemoveFromContext();

		protected virtual void CancelSpawnCard()
		{
			RemoveFromContext();
		}

		protected virtual void ConfirmSpawnCard()
		{
			if (Data.IsSpawnedByResolver)
				return;
			AddEffectsPostSpawn();
			BatchController.PlayCards(Data);
		}

		protected virtual void AddEffectsPostSpawn()
		{
			if (!Data.EffectsContainer.Has(EffectKeyword.Sleeping))
				Data.EffectsContainer.AddFirst(EffectKeyword.Sleeping);
		}
	}
}