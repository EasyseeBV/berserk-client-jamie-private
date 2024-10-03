using System.Collections;
using System.Collections.Generic;
using Berserk.Shared.Data.Enums;
using DG.Tweening;
using Events;
using Game.Effect_System;
using Game.Effect_System.Target;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using Vulcan.Data;
using Vulcan.Network;
using EffectPhase = Vulcan.Data.EffectPhase;
using Owner = Berserk.Shared.Data.Enums.Owner;

namespace Game.Entities
{
	public abstract class InteractiveEntity<T> : MonoEntityBase<T>, IInteractiveEntity, IPointerEnterHandler, IPointerExitHandler,
		IBeginDragHandler, IDragHandler, IPointerClickHandler where T : DataBase
	{
		protected bool isCanAttack;
		protected bool isSelectable = true;

		public virtual bool IsCanAttack
		{
			get => isCanAttack;
			set
			{
				isCanAttack = value && !Data.EffectsContainer.Has(EffectKeyword.Stunning);
				UpdateSelect();
			}
		}

		public bool IsAllowedAttack { get; set; } = true;

		public EntityView EntityView => (EntityView)View;

		protected virtual void Awake()
		{
			GameBus.OnPassingTurned.Subscribe(this, OnPassingTurned);
		}

		protected override void OnInit()
		{
			Data.Hp.OnChangedFrom += OnHpChanged;
		}

		protected override void OnDeath()
		{
			if (IsDead)
				return;
			base.OnDeath();

			if (Data != null)
				Data.Hp.OnChangedFrom -= OnHpChanged;
		}

		public virtual void Attack(IInteractiveEntity target, bool defenceDamage = true)
		{
			StartCoroutine(YieldAttack(target, defenceDamage));
		}

		public virtual IEnumerator YieldAttack(IInteractiveEntity target, bool defenceDamage = true)
		{
			if (IsInvalidAttack())
			{
				isSelectable = true;
				UpdateSelect();
				yield break;
			}

			target.IsAllowedAttack = false;
			isSelectable = false;

			RRLogger.Log($"{DataBase.Title}".Blue() + $": {DataBase.Hp} - Before Attack");
			EffectHandler.Handle(this, EffectPhase.BeforeAttack, target);

			yield return EntityView
				.AnimateAttack(target.EntityView, DoDamage, DoDamageResponseAndComplete)
				.WaitForCompletion();

			void DoDamage()
			{
				//Check double call attack on fast play
				if (target.IsDead)
					return;

				IsCanAttack = false;
				target.ApplyDamage(this, Data.Attack);
			}

			void DoDamageResponseAndComplete()
			{
				target.IsAllowedAttack = !target.IsDead;
				//Do not process if the attack has not yet occurred
				if (IsCanAttack)
					return;

				var canDefence = defenceDamage && !Data.EffectsContainer.Has(EffectKeyword.Immortal);

				if (canDefence)
				{
					EffectHandler.Handle(target, EffectPhase.OnBeforeDefence, this);

					if (!target.DataBase.EffectsContainer.Has(EffectKeyword.Stunning))
						ApplyDamage(target, target.DataBase.Attack);
				}
				
				BatchController.PlayerPerformAction(this, new[] { target }, ActionType.Attack, defenceDamage: defenceDamage);

				if (canDefence)
					EffectHandler.Handle(target, EffectPhase.OnAfterDefence, this);

				EffectHandler.Handle(this, EffectPhase.AfterAttack, target);
				isSelectable = true;
			}

			bool IsInvalidAttack()
			{
				return target == null
				       || target.DataBase.EffectsContainer.Has(EffectKeyword.Immortal)
				       || Data.EffectsContainer.Has(EffectKeyword.Stunning)
				       || IsDead
				       || target.IsDead
				       || !target.IsAllowedAttack;
			}
		}

		public void ApplyDamage(IInteractiveEntity source, int value)
		{
			if (IsDead)
				return;
			
			if (Data.EffectsContainer.Has(EffectFamily.IgnoreDamage))
				return;

			//Need to check for armoring before OnBeforeDamaged effects due effects can be removed
			if (Data.EffectsContainer.Has(EffectFamily.AbsorbDamage))
			{
				EffectHandler.Handle(this, EffectPhase.OnBeforeDamaged, source);
				return;
			}

			EffectHandler.Handle(this, EffectPhase.OnBeforeDamaged, source);

			if (Data.Hp - value > Data.Hp.GetMax())
				Data.Hp.SetAboveMax(Data.Hp - value);
			else
				Data.Hp.Add(-value); // will be clamped to [0,max]

			if (Data.Hp.GetPrevious != Data.Hp)
				HandleDamage(IsDead);

			void HandleDamage(bool isDead)
			{
				// The delay is needed to wait for the position of the opponent's return animation.
				// To take the final entity position for effects processing.
				DOVirtual.DelayedCall(1f, () =>
				{
					EffectHandler.Handle(this, EffectPhase.OnAfterDamaged, source);
					if (isDead)
						EffectHandler.Handle(this, EffectPhase.AfterDead);
				});
			}
		}

		public void KillSelf()
		{
			Data?.Hp.Add(-Data.Hp);
		}

		public abstract void SetTurn(bool myTurn);

		protected abstract void OnPassingTurned();

		protected void Sync()
		{
			BatchController.ModifySelfEntity(this);
		}

		private void UpdateSelect()
		{
			if (isCanAttack)
				View?.Select(Color.white);
			else
				View?.Unselect();
		}

		private void OnHpChanged(int from, int to)
		{
			RRLogger.Log($"{DataBase.Title}" + $": {to}");

			if (Data.Hp > 0)
				return;

			EffectHandler.Handle(this, EffectPhase.BeforeDead);

			OnDeath();

			GameBus.OnEntityDie += Data;

			Destroy();
		}

		protected override void Destroy()
		{
			gameObject.SetActive(false);

			// await Sync
			Destroy(gameObject, 10f);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			GameBus.OnEntityTarget += this;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			GameBus.OnEntityTarget += null;
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			if (!isSelectable)
				return;

			if (!(GameBus.CurrentRound.Value is { TurnOwner: Owner.Self }))
				return;

			if (Owner != Owner.Self || !IsCanAttack)
				return;

			var picksInfo = new List<PickInfo>(1) { new PickInfo { From = this } };
			TargetResolver.GetManualTarget(picksInfo, PlayAttack, () => View.Select(Color.white));

			void PlayAttack()
			{
				isSelectable = false;
				CommandController.Enqueue(() =>
				{
					var pickInfo = picksInfo[0];
					var target = pickInfo.Targets[0];

					if (target == null || target.IsDead)
					{
						GameBus.OnActionBlocked += BlockedInfo.NoCreatureTarget;
						isSelectable = true;
						UpdateSelect();
						return;
					}

					if (pickInfo.From.IsDead)
						return;

					Attack(target);
				});
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
		}

		public void OnPointerClick(PointerEventData eventData)
		{
#if UNITY_EDITOR
			RRLogger.Log($"[{"Info".Green().Bold()}] {Data.Title} => [{Data.UID.Green()}] "
			             + $"Effects - [{Data.EffectsContainer.ToString().Green()}]\n"
			             + $"HP - [{Data.Hp.ToString().Green()}]\n"
			             + $"Attack - [{Data.Attack.ToString().Green()}]\n"
			             + $"Mana - [{Data.Lava.ToString().Green()}]\n"
			             + $"IsCanAttack - [{isCanAttack.ToString().Green()}]\n"
			             + $"IsSpawnedByResolver - [{DataBase.IsSpawnedByResolver.ToString().Green()}]\n"
			             + $"IsSpawnedByEffect - [{DataBase.IsSpawnedByEffect.ToString().Green()}]\n"
			             + $"Owner - [{DataBase.Owner.ToString().Green()}]\n");
#endif
		}
	}
}