using System.Linq;
using Berserk.Shared.Data.Enums;
using BerserkV3.Lobby;
using BerserkV3.Startup.Authorization;
using Events;
using Game.Entities;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using ServerCore.Infrastructure.Models;
using Vulcan.Data;
using Vulcan.VFX;
using CardData = Vulcan.Data.CardData;

namespace Vulcan.Network.Resolver
{
	public static class ResolverHelpers
	{
		public static Owner GetOwnerByUsername(string userName)
		{
			return userName == User.UserName ? Owner.Self : Owner.Opponent;
		}

		public static bool CanResolve(EntityStateModel remoteState)
		{
			if (remoteState == null)
			{
				RRLogger.Error($"[{"Resolver".Orange().Bold()}] {nameof(remoteState)} is null");
				return false;
			}

			if (!CheckEntityByID(remoteState.Id))
			{
				RRLogger.Error(
					$"[{"Resolver".Orange().Bold()}] {nameof(IInteractiveEntity)} - {remoteState.Id} is null");
				RRLogger.Error($"[{"Resolver".Orange().Bold()}] {"Need Update Context".Red().Bold()}");
				return false;
			}

			return true;
		}

		private static bool CheckEntityByID(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				RRLogger.Error($"[{"Resolver".Orange().Bold()}] Local entity ID is null");
				return false;
			}

			if (TryFindEntityOnBoard(id, out var entity))
				return true;

			RRLogger.Log($"[{"Resolver".Orange().Bold()}] Local entity on board not found - {id}");

			if (TryFindEntityOverboard())
				return true;

			RRLogger.Error($"[{"Resolver".Orange().Bold()}] Local entity overboard not found {id}");
			return false;

			bool TryFindEntityOverboard()
			{
				return FindCardById(id) != null;
			}
		}

		public static bool TryFindEntityOnBoard(string id, out IInteractiveEntity interactiveEntity)
		{
			interactiveEntity = GameBus.LocalContext.GetAllTargetEntities().FirstOrDefault(x => x.DataBase.UID == id);
			return interactiveEntity != null;
		}

		public static IInteractiveEntity CreateEntityMock(string id, InteractiveCardModel model)
		{
			var localData = FindCardById(id) ?? model.ToCardData();

			RRLogger.Log($"[{"Resolver".Orange().Bold()}] Create entity mock {localData?.Title} - {localData?.UID}");

			return new InteractiveEntityMock(localData);
		}

		public static bool IsPlayerSelf(SessionPlayerModel player)
		{
			return player.UserName == User.UserName;
		}

		public static bool ValidateEntity(IInteractiveEntity entity, EntityStateModel remoteState)
		{
			var flag = entity.DataBase.UID == remoteState.Id
			           && entity.DataBase.Hp.GetMax() == remoteState.MaxHealth
			           && entity.DataBase.Hp == remoteState.CurrentHealth
			           && entity.DataBase.Attack.GetMax() == remoteState.MaxAttack
			           && entity.DataBase.Attack == remoteState.CurrentAttack
			           && entity.DataBase.Lava.GetMax() == remoteState.MaxMana
			           && entity.DataBase.Lava == remoteState.CurrentMana
			           && entity.IsCanAttack == remoteState.IsCanAttack
			           && entity.DataBase.EffectsContainer.Values.Select(x => (x.Id, x.Length, x.Value))
				           .SequenceEqual(remoteState.CurrentEffects.Select(x => (x.EffectId, x.Length, x.Value)));
#if UNITY_EDITOR
			if (!flag)
			{
				var log =
					$"[{"Resolver".Orange().Bold()}] {"Warning".Red().Bold()} Validation failed {entity.DataBase.UID}\n" +
					$"{entity.DataBase.Title}\n";
				if (entity.DataBase.UID != remoteState.Id)
					log += $"Id [{entity.DataBase.UID}] -> [{remoteState.Id}]\n";

				if (entity.DataBase.Attack.GetMax() != remoteState.MaxAttack)
					log += $"MaxAttack [{entity.DataBase.Attack.GetMax()}] -> [{remoteState.MaxAttack}]\n";

				if (entity.DataBase.Attack != remoteState.CurrentAttack)
					log += $"Attack [{entity.DataBase.Attack}] -> [{remoteState.CurrentAttack}]\n";

				if (entity.DataBase.Hp.GetMax() != remoteState.MaxHealth)
					log += $"MaxHP [{entity.DataBase.Hp.GetMax()}] -> [{remoteState.MaxHealth}]\n";

				if (entity.DataBase.Hp != remoteState.CurrentHealth)
					log += $"HP [{entity.DataBase.Hp}] -> [{remoteState.CurrentHealth}]\n";

				if (entity.DataBase.Lava.GetMax() != remoteState.MaxMana)
					log += $"MaxMana [{entity.DataBase.Lava.GetMax()}] -> [{remoteState.MaxMana}]\n";

				if (entity.DataBase.Lava != remoteState.CurrentMana)
					log += $"Mana [{entity.DataBase.Lava}] -> [{remoteState.CurrentMana}]\n";

				if (entity.IsCanAttack != remoteState.IsCanAttack)
					log += $"IsCanAttack [{entity.IsCanAttack}] -> [{remoteState.IsCanAttack}]\n";

				if (!entity.DataBase.EffectsContainer.Values.Select(x => (x.Id, x.Length, x.Value))
					    .SequenceEqual(remoteState.CurrentEffects.Select(x => (x.EffectId, x.Length, x.Value))))
					log += $"Effects [{entity.DataBase.EffectsContainer}] -> [{RemoteEffectsToString()}]\n";

				RRLogger.Error(log);

				string RemoteEffectsToString()
				{
					return remoteState
						.CurrentEffects
						.Aggregate("", (result, x) => result + $"[{x.Effect}, {x.Id}, {x.EffectId}, Length= {x.Length} , Value= {x.Value}]\n");
				}
			}
			else
			{
				RRLogger.Log($"[{"Resolver".Orange().Bold()}] {"Vailidate success".Green().Bold()} - {entity.DataBase.UID}\n");
			}
#endif
			return flag;
		}

		private static CardData FindCardById(string id)
		{
			return GameBus.LocalContext.HandCardsList
				       .FirstOrDefault(x => x.Data.UID == id)?.Data
			       ?? GameBus.LocalContext.GraveyardCardsList
				       .FirstOrDefault(e => e.UID == id);
		}

		public static void UpdateEntity(IInteractiveEntity entity, EntityStateModel remoteState)
		{
			entity.DataBase.UID = remoteState.Id;
			entity.DataBase.Hp.SetMax(remoteState.MaxHealth);
			entity.DataBase.Hp.Set(remoteState.CurrentHealth);
			entity.DataBase.Attack.SetMax(remoteState.MaxAttack);
			entity.DataBase.Attack.Set(remoteState.CurrentAttack);
			entity.DataBase.Lava.SetMax(remoteState.MaxMana);
			entity.DataBase.Lava.Set(remoteState.CurrentMana);
			entity.IsCanAttack = remoteState.IsCanAttack;
			ResolveEffects();

			void ResolveEffects()
			{
				var localEffects = entity.DataBase.EffectsContainer.Values;
				localEffects.Clear();
				localEffects.AddRange(remoteState.CurrentEffects.Select(x =>
				{
					var effectState = x.ToEffectStateModel();
					VFXController.Spawn(new EffectInfo(entity, effectState.EffectData), isRemote: true);
					return effectState;
				}));
			}
		}
	}
}