using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;
using BerserkV3.Common.DataBase;
using Events;
using Game.Entities;
using RR.Core.Extensions;
using Vulcan.Data;
using CardData = Vulcan.Data.CardData;
using EffectData = Vulcan.Data.EffectData;
using EffectTargetLimit = Vulcan.Data.EffectTargetLimit;
using EffectTargetMod = Vulcan.Data.EffectTargetMod;
using Owner = Berserk.Shared.Data.Enums.Owner;

namespace Game.Effect_System.Target
{
	public class ConditionHandler
	{
		public IEnumerable<IInteractiveEntity> GetAOETargets(IInteractiveEntity owner, EffectData effectData)
		{
			return GameBus.LocalContext.Vulcanites.OfType<IInteractiveEntity>()
				.Concat(GameBus.LocalContext.TableCardsList)
				.Where(target => AllowedTarget(owner, target, effectData, unmanageable:true));
		}

		public bool IsSpellPlayAllowed(IMonoEntity owner, EffectData effectData)
		{
			if (effectData.TargetMod != EffectTargetMod.None)
				return GetRandomAllowedTarget(owner, effectData) != null;

			//TODO: add new EffectTargetMod or other
			//Summon (Always for Summon EffectTargetMod == None)
			var adaptedOwner = GetAdaptedOwner(owner, effectData);
			return HasBoardSpace(adaptedOwner) && CheckGraveyardCreepsLimitEntity(effectData.TargetLimit, adaptedOwner);
		}

		/// <summary>
		/// Returns random target from allowed by effect options.
		/// </summary>
		/// <returns></returns>
		public IInteractiveEntity GetRandomAllowedTarget(IMonoEntity owner, EffectData effectData, bool isCheckDead = false)
		{
			return GameBus.LocalContext.GetAllTargetEntities()
				.Where(c => !isCheckDead || c.DataBase.Hp > 0)
				.Where(c => AllowedTarget(owner, c, effectData, unmanageable: true))
				.Shuffle()
				.FirstOrDefault();
		}

		/// <summary>
		/// Returns random target except self from allowed by effect options.
		/// </summary>
		/// <returns></returns>
		public IInteractiveEntity GetRandomAllowedTargetExceptSelf(IInteractiveEntity owner, EffectData effectData, bool isCheckDead = false)
		{
			return GameBus.LocalContext.GetAllTargetEntities()
				.Where(c => c.DataBase.UID != owner.DataBase.UID)
				.Where(c => !isCheckDead || c.DataBase.Hp > 0)
				.Where(c => AllowedTarget(owner, c, effectData, unmanageable: true))
				.Shuffle()
				.FirstOrDefault();
		}

		/// <summary>
		/// Provides conditions for selecting targets when using effects
		/// </summary>
		public bool AllowedTarget(IMonoEntity owner, IInteractiveEntity target, EffectData effectData, bool unmanageable = false)
		{
			return (effectData.Effect == EffectKeyword.Silence || CheckImmune(owner, target))
			     && CheckStealth(owner, target, unmanageable)
			     && AllowedTargetLimit()
			     && AllowedTargetOwner();

			bool AllowedTargetLimit() =>
				effectData.TargetLimit == EffectTargetLimit.None
				|| effectData.TargetLimit == EffectTargetLimit.Hero && target.DataBase is ActorData
				|| effectData.TargetLimit == EffectTargetLimit.Cards && target.DataBase is CardData;

			bool AllowedTargetOwner() =>
				effectData.TargetOwner == Owner.None
				|| owner.DataBase.Owner == Owner.Self && effectData.TargetOwner == target.DataBase.Owner
				|| owner.DataBase.Owner == Owner.Opponent && effectData.TargetOwner != target.DataBase.Owner;
		}

		/// <summary>
		/// Provides conditions for selecting targets when using an attack
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="target"></param>
		public bool AllowedTarget(IInteractiveEntity owner, IInteractiveEntity target)
		{
			return target.DataBase.Owner != owner.DataBase.Owner
			       && CheckImmune(owner, target) 
			       && CheckStealth(owner, target)
			       && CheckTaunt(target);
		}


		public bool HasBoardSpace(Owner owner) => GameBus.LocalContext.GetAliveTableCardsByOwner(owner).Count() < SharedConfigAdapter.Config.MaxCardsOnTable;

		private bool CheckGraveyardCreepsLimitEntity(EffectTargetLimit limit, Owner targetOwner)
		{
			//No graveyard creeps
			if (limit == EffectTargetLimit.None)
				return true;

			var adaptedLimit = GetAdaptedGraveyardOwner(targetOwner, limit);

			if (adaptedLimit != EffectTargetLimit.Graveyard
				&& adaptedLimit != EffectTargetLimit.GraveyardSelf
				&& adaptedLimit != EffectTargetLimit.GraveyardOpponent)
				return true;

			var result = false;
			if (adaptedLimit != EffectTargetLimit.GraveyardOpponent)
				result = HasCreeps(Owner.Self);
			if (adaptedLimit != EffectTargetLimit.GraveyardSelf)
				result = result || HasCreeps(Owner.Opponent);
			return result;

			bool HasCreeps(Owner owner) =>
				GameBus.LocalContext
				.GetGraveyardCardsByOwner(owner)
				.Any(c => c.Type == CardType.Creep);
		}

		private bool CheckTaunt(IInteractiveEntity target)
		{
			var flag = target.DataBase.EffectsContainer.Has(EffectKeyword.Taunting)
				   || GameBus.LocalContext.GetTableCardsByOwner(target.DataBase.Owner)
					   .All(x => !x.DataBase.EffectsContainer.Has(EffectKeyword.Taunting));
			if (!flag)
				GameBus.OnActionBlocked += BlockedInfo.ShouldTargetTaunt;
			return flag;
		}

		private bool CheckImmune(IMonoEntity owner, IInteractiveEntity target)
		{
			var flag = !target.DataBase.EffectsContainer.Has(EffectKeyword.Immortal) || owner.DataBase.Owner == target.DataBase.Owner;
			if (!flag)
				GameBus.OnActionBlocked += BlockedInfo.Immune;
			return flag;
		}

		private bool CheckStealth(IMonoEntity owner, IMonoEntity target, bool unmanageable = false)
		{
			return !target.DataBase.EffectsContainer.Has(EffectKeyword.Stealthing)
			       || unmanageable
			       || owner.DataBase.Owner == target.DataBase.Owner;
		}

		private Owner GetAdaptedOwner(IMonoEntity owner, EffectData effectData)
		{
			var adaptedOwner = Owner.None;
			if (owner.DataBase.Owner == Owner.Self)
				adaptedOwner = effectData.TargetOwner == Owner.Self
					? Owner.Self
					: Owner.Opponent;

			if (owner.DataBase.Owner == Owner.Opponent)
				adaptedOwner = effectData.TargetOwner == Owner.Self
					? Owner.Opponent
					: Owner.Self;

			return adaptedOwner;
		}

		private EffectTargetLimit GetAdaptedGraveyardOwner(Owner targetOwner, EffectTargetLimit limit)
		{
			var adaptedLimit = EffectTargetLimit.None;
			if (targetOwner == Owner.Self)
				adaptedLimit = limit == EffectTargetLimit.GraveyardSelf
					? EffectTargetLimit.GraveyardSelf
					: EffectTargetLimit.GraveyardOpponent;

			if (targetOwner == Owner.Opponent)
				adaptedLimit = limit == EffectTargetLimit.GraveyardOpponent
					? EffectTargetLimit.GraveyardOpponent
					: EffectTargetLimit.GraveyardSelf;

			return adaptedLimit;
		}
	}
}