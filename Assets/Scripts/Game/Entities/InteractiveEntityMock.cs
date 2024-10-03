using System;
using System.Collections;
using Berserk.Shared.Data.Enums;
using RR.Core.DebugSystem;
using UI;
using UnityEngine;
using Vulcan.Data;
using CardData = Vulcan.Data.CardData;

namespace Game.Entities
{
	public class InteractiveEntityMock : IInteractiveEntity
	{
		protected bool isCanAttack;

		public bool IsCanAttack
		{
			get => isCanAttack;
			set => isCanAttack = value && !Data.EffectsContainer.Has(EffectKeyword.Stunning);
		}

		public bool IsAllowedAttack { get; set; }
		public bool IsCanUseAbility { get; set; } = true;

		public RectTransform RectTransform => null;
		public DataBase DataBase => Data;
		public bool IsDead { get; }
		public CardData Data { get; }
		public EntityView EntityView => null;
		public MonoEntityBaseView View => null;

		public InteractiveEntityMock(CardData data)
		{
			if (data == null)
				throw new NullReferenceException();

			Data = data;
		}

		public void ApplyDamage(IInteractiveEntity source, ActionType attackType, int value, bool triggerEffects)
		{
			RRLogger.Error($"{nameof(ApplyDamage)} failed, this is a mock, an entity out of the board");
		}

		public void Attack(IInteractiveEntity target, bool defenceDamage = true)
		{
			RRLogger.Error($"{nameof(Attack)} failed, this is a mock, an entity out of the board");
		}

		public IEnumerator YieldAttack(IInteractiveEntity target, bool defenceDamage = true)
		{
			RRLogger.Error($"{nameof(YieldAttack)} failed, this is a mock, an entity out of the board");
			yield break;
		}

		public void KillSelf()
		{
			RRLogger.Error($"{nameof(KillSelf)} failed, this is a mock, an entity out of the board");
		}

		public void ApplyDamage(IInteractiveEntity source, int value)
		{
			RRLogger.Error($"{nameof(ApplyDamage)} failed, this is a mock, an entity out of the board");
		}

		public void OnTurnChanged(bool myTurn)
		{
			RRLogger.Error($"{nameof(OnTurnChanged)} failed, this is a mock, an entity out of the board");
		}
	}
}