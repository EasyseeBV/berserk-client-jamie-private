using System;
using Audio;
using Berserk.Shared.Data.Enums;
using DG.Tweening;
using Events;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using UnityEngine;
using Vulcan.Audio;
using Vulcan.Data;
using Vulcan.VFX;

namespace UI
{
	public abstract class EntityView : MonoEntityBaseView
	{
		[SerializeField] protected Transform entityContainer;
		[SerializeField] protected Transform entityTransform;

		protected DataBase Data;

		protected Vector2 position;
		protected bool isAttacking;
		private Action onSetPosition;
		private Tween takeHitTween;
		private Tween takeHealTween;

		private bool isDead;

		public override void SetUp(DataBase data)
		{
			Data = data;
			Data.Hp.OnChangedFrom += ShowVFXHpChange;
			SubscribeOnDataChange();
			GameBus.OnEntityDie.Subscribe(this, x => ReferenceEquals(x, Data), OnDeath);
			Unselect();
		}

		public override void Select(Color selectedColor)
		{
		}

		public override void Unselect()
		{
		}

		public override void SelectOnRequestingTarget(Color color)
		{
		}

		public override void SetPosition(Vector3 pos, Quaternion rot)
		{
			position = pos;
			onSetPosition?.Invoke();
		}

		public Sequence AnimateAttack(EntityView target, Action onAttack, Action onDefence)
		{
			var scaleTo = Vector3.one * 1.5f;
			var scaleFrom = Vector3.one;
			isAttacking = true;
			entityTransform.SetAsLastSibling();

			if (target.Value()?.RectTransform.Value()?.position == null)
				RRLogger.Error("Target RectTransform == null");

			var attackMove = entityTransform
				.DOMove(target.Value()?.RectTransform.Value()?.position ?? RectTransform.position, 0.1f)
				.SetDelay(0.05f)
				.SetEase(Ease.InCirc)
				.OnComplete(() =>
				{
					AudioController.Play(Clip.TableCard_Attack);
					onAttack.Invoke();
				});

			var returnMove = entityTransform
				.DOLocalMove(position, 0.3f)
				.SetEase(Ease.OutCubic);

			// subscribe to update tweens position on table
			onSetPosition += UpdateTweens;
			var sequence = DOTween.Sequence()
				.Append(entityTransform.DOScale(scaleTo, 0.4f).SetEase(Ease.InOutCubic))
				.Append(attackMove)
				.Append(returnMove)
				.OnComplete(() =>
				{
					// unsubscribe ater move tweens complete
					onSetPosition -= UpdateTweens;
					onDefence.Invoke();
				})
				.Append(entityTransform
					.DOScale(scaleFrom, 0.3f)
					.SetEase(Ease.InOutCubic)
					.OnComplete(() => isAttacking = false))
				.Play();

			return sequence;

			// if sequence not completed but, SetPosition alredy changed.
			// Update all tweens to correct work animation;
			void UpdateTweens()
			{
				if (attackMove != null)
					attackMove.endValue = target.Value()?.RectTransform.Value()?.position ?? RectTransform.position;

				if (returnMove != null)
					returnMove.endValue = position;
			}
		}

		protected virtual void OnDeath(DataBase data)
		{
			if (isDead)
				return;

			isDead = true;
			if (Data == null)
			{
				RRLogger.Warning($"[{"EntityView".Blue().Bold()}] OnDeath EntityView.Data is null");
				return;
			}

			Data.Hp.OnChangedFrom -= ShowVFXHpChange;
			UnsubscribeOnDataChange();

			if (data == null)
			{
				RRLogger.Warning($"[{"EntityView".Blue().Bold()}] OnDeath(DataBase data) data is null");
				return;
			}

			var safePosition = transform
				? (Vector2)transform.position
				: position;
			// VFXController.Spawn(safePosition, EffectVisualKeyword.Destruction, data.Id);
		}

		protected abstract void SubscribeOnDataChange();

		protected abstract void UnsubscribeOnDataChange();

		protected virtual void ShowVFXHpChange(int from, int to)
		{
			if (from > to)
			{
				var safePosition = RectTransform
					? RectTransform.position
					: (Vector3)position;

				GameBus.OnDamageDealt.Publish(safePosition, from - to);
				TakeHitAnimation();
			}
			else if (from < to)
			{
				TakeHealAnimation();
			}
		}

		protected virtual void TakeHitAnimation()
		{
			AudioController.Play(Clip.TableCard_Damage);
			takeHitTween?.Kill();
			if (entityContainer)
				takeHitTween = entityContainer
					.DOShakePosition(0.75f, 12)
					.OnKill(() => entityContainer.localPosition = Vector3.zero);
		}

		protected virtual void TakeHealAnimation()
		{
			AudioController.Play(Clip.TableCard_Heal);
			takeHealTween?.Kill();
			if (entityContainer)
				takeHealTween = entityContainer
					.DOScale(0.75f, .75f)
					.SetLoops(2, LoopType.Yoyo)
					.OnKill(() => entityContainer.localScale = Vector3.one);
		}

		protected virtual void OnDestroy()
		{
			if (Data != null)
				Data.Hp.OnChangedFrom -= ShowVFXHpChange;
			OnDeath(null);
		}
	}
}