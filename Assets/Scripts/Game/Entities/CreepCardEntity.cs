using Berserk.Shared.Data.Enums;
using Events;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using UI;
using UnityEngine;

namespace Game.Entities
{
	public class CreepCardEntity : TableCardEntity
	{
		public override bool IsCanAttack
		{
			get => isCanAttack;
			set => base.IsCanAttack = value && !IsSleep;
		}

		public bool IsSleep => Data.EffectsContainer.Has(EffectKeyword.Sleeping);


		protected override void OnInit()
		{
			base.OnInit();

			name = $"{Data.Id}:{Data.Title}_tableCard";
			transform.localRotation = Quaternion.identity;

			AddToContext();
		}

		public void UpdateView(bool hasTaunt)
		{
			View.UpdateFace(hasTaunt ? TableCardFaceId.Taunt : TableCardFaceId.Default);
		}

		protected override void OnDeath()
		{
			if (IsDead)
				return;

			base.OnDeath();
			GameBus.LocalContext.AddGraveyardCard(Data);
		}

		protected override void CancelSpawnCard()
		{
			if (Data.IsSpawnedByEffect)
			{
				ConfirmSpawnCard();
				return;
			}

			base.CancelSpawnCard();
			Destroy(gameObject);
		}

		protected override void AddToContext()
		{
			GameBus.LocalContext.AddTableCard(this);
		}

		protected override void RemoveFromContext()
		{
			GameBus.LocalContext.RemoveTableCard(this);

			RRLogger.Log("Canceled SpawnCreepCard=" + string.Join(", ", Data.EffectsContainer.Values).Lightblue());
		}

		public override void SetTurn(bool myTurn)
		{
			IsCanAttack = myTurn;
		}

		protected override void OnPassingTurned()
		{
			IsCanAttack = false;
		}
	}
}