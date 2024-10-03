using System.Collections;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Events;
using Game.Effect_System;
using Game.Effect_System.Target;
using RR.Core.DebugSystem;
using UI;
using UnityEngine;
using Vulcan.Data;
using Vulcan.Network;
using EffectData = Vulcan.Data.EffectData;
using EffectPhase = Vulcan.Data.EffectPhase;
using Owner = Berserk.Shared.Data.Enums.Owner;

namespace Game.Entities
{
	public class ActorEntity : InteractiveEntity<ActorData>
	{
		[SerializeField]
		private Owner owner;

		public override Owner Owner
		{
			get => owner;
			protected set => owner = value;
		}

		public override bool IsCanAttack
		{
			get => isCanAttack;
			set => isCanAttack = value && Data.Attack > 0 && !Data.EffectsContainer.Has(EffectKeyword.Stunning);
		}

		protected new ActorView View => (ActorView)base.View;

		private void Start()
		{
			GameBus.LocalContext.AddVulcanite(this);

			//Syncs with the server, spending mana or using the vulcanite ability
			GameBus.OnSpawnConfirmed.Subscribe(this,
				x => Owner == Owner.Self && !x.IsSpawnedByEffect, Sync);
			// Refund spent mana without syncing with the server
			GameBus.OnSpawnCanceled.Subscribe(this,
				x => x.Owner == Data.Owner // return mana only to the owner
				     && !x.IsSpawnedByEffect // Do not return mana when using the summon. They are free
				     && !x.Equals(DataBase), // Do not return mana when using the ability. They are free
				x => AddLava(x.Lava));
			// For spawn creep effects
			GameBus.OnTimerPaused.Subscribe(this, UpdateAbilityState);
			GameBus.OnSpawnConfirmed.Subscribe(this, x => x.Equals(DataBase), UpdateAbilityState);
			GameBus.OnSpawnCanceled.Subscribe(this, x => x.Equals(DataBase), UpdateAbilityState);
			GameBus.TargetSelecting.Subscribe(this, UpdateAbilityState);
			GameBus.OnPickedConfirmed.Subscribe(this, UpdateAbilityState);
		}

		protected override void OnInit()
		{
			base.OnInit();
			UpdateAbilityState();
		}

		public void InitActorFromRemote(ActorData actorData)
		{
			Init(actorData);

			Data.Attack.OnChanged += OnAttackChanged;
			View.OnAbilityButtonActivated += () =>
			{
				var effect = GetAvailableAbility(out var canUse);
				if (effect == null || !canUse)
				{
					UpdateAbilityState();
					return;
				}

				if (!TargetResolver.ConditionHandler.IsSpellPlayAllowed(this, effect))
				{
					GameBus.OnActionBlocked += BlockedInfo.NoSpellTarget;
					return;
				}

				View.SetAbilityInteractable(false);
				CommandController.Enqueue(() =>
				{
					EffectHandler.Handle(this, EffectPhase.OnAbilityButtonPress);
					UpdateAbilityState();
				});
			};
		}

		public void SpendLava(int manaDelta)
		{
			if (Data.Lava - manaDelta < 0)
				RRLogger.Error("Mana cant be lower than 0");

			AddLava(-manaDelta);
		}

		public void AddLava(int manaDelta)
		{
			Data.Lava.AddAboveMax(manaDelta);
			GameBus.OnLavaChanged.Publish(Owner, Data.Lava);
		}

		private void OnAttackChanged(int currentAttack)
		{
			IsCanAttack = currentAttack > 0;
		}

		public void UpdateArmor(int value)
		{
			View.UpdateArmor(value);
		}

		public override void SetTurn(bool myTurn)
		{
			View.OnTurnChange(myTurn);
			View.AdjustAbilityButton(GetAvailableAbility(out var canUse), canUse && myTurn);
		}

		private void UpdateAbilityState()
		{
			View.AdjustAbilityButton(GetAvailableAbility(out var canUse), canUse);
		}

		private EffectData GetAvailableAbility(out bool canUse)
		{
			var heroAbilities = Data.EffectsContainer.GetHeroAbilities();
			// take first ability with length > 0 or take anyway any ability
			var effectState = heroAbilities.FirstOrDefault(x=>x.Length != 0) ?? heroAbilities.FirstOrDefault();
			
			canUse = !GameBus.TargetSelecting
			         && effectState != null
			         && effectState.Length != 0 // can be -1
			         && effectState.EffectData.Phase == EffectPhase.OnAbilityButtonPress // use only on press
			         && GameBus.CurrentRound.Value.TurnOwner == Owner.Self
			         && Data.Owner == Owner.Self
			         && !Data.EffectsContainer.Has(EffectKeyword.Stunning)
			         && !GameBus.OnTimerPaused;
			
			return effectState?.EffectData;
		}
		
		protected override void OnPassingTurned()
		{
			if(!View)
				return;
			
			View.OnTurnChange(false);
			IsCanAttack = false;
			View.SetAbilityInteractable(false);
		}

		protected override void Destroy()
		{
			View.OnDie();
			// Check if the server sends a signal about the death of the vulcanite
		}

		private IEnumerator HandleActorDeath()
		{
			Data.Attack.OnChanged -= OnAttackChanged;
			yield return null;
			GameBus.OnVulcaniteDies += Data;
		}
	}
}