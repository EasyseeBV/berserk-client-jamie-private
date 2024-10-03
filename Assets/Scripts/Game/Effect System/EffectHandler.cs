using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Berserk.Shared.Data.Enums;
using Events;
using Game.Effect_System.Effects;
using Game.Effect_System.Target;
using Game.Entities;
using Newtonsoft.Json;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Vulcan.Data;
using Vulcan.Network;
using Vulcan.Network.Context;
using EffectPhase = Vulcan.Data.EffectPhase;
using EffectTargetMod = Vulcan.Data.EffectTargetMod;
using Owner = Berserk.Shared.Data.Enums.Owner;

namespace Game.Effect_System
{
	public class EffectHandler : MonoBehaviour
	{
		[ShowInInspector, ReadOnly]
		public static Dictionary<EffectKeyword, IEffectBehaviour> Effects;

		public static void Resolve(IInteractiveEntity owner, EffectPhase phase,
			EffectKeyword effect = EffectKeyword.None,
			params IInteractiveEntity[] targets)
		{
			EffectState[] effects;
			if (effect != EffectKeyword.None)
				effects = owner.DataBase.EffectsContainer.Values
					.Where(x => x.EffectData.Effect == effect)
					.ToArray();
			else
				effects = owner.DataBase.EffectsContainer.Values
					.Where(x => x.EffectData.Phase == phase)
					.ToArray();

			Handle(owner, targets ?? Array.Empty<IInteractiveEntity>(), effects, true);
			RRLogger.Log(
				$"[{"EffectHandler".Blue().Bold()}] Handle resolve effect {effect}\n" +
				$"From: {owner.DataBase.Title}, {owner.DataBase.Id}, {owner.DataBase.UID.Green()}\n");
		}

		public static void Handle(IInteractiveEntity owner, EffectPhase phase, params IInteractiveEntity[] targets)
		{
			var phaseEffects = owner.DataBase.EffectsContainer.Values
				.Where(x => x.EffectData.Phase == phase)
				.OrderByDescending(e => e.EffectData.Effect)
				.ToArray();

			if (!phaseEffects.Any())
				return;

			Handle(owner, targets ?? Array.Empty<IInteractiveEntity>(), phaseEffects, false);
			RRLogger.Log(
				$"[{"EffectHandler".Blue().Bold()}] Handle local effects: " +
				$"{string.Join("\n", phaseEffects.Select(x => $"{x.EffectData.Id}, {x.EffectData.Effect}, {x.EffectData.VisualEffect}"))}\n" +
				$"From: {owner.DataBase.Title}, {owner.DataBase.Id}, {owner.DataBase.UID.Green()}\n");
		}

		private static void Handle(IInteractiveEntity owner, IInteractiveEntity[] targets, EffectState[] effects,
			bool isFromResolver)
		{
			var picksInfoList = effects
				.Select(state => new PickInfo(state, owner.GetRealOwner())
				{
					Targets = targets?.Where(x => x != null).ToArray() ?? Array.Empty<IInteractiveEntity>(),
					IsFromResolver = isFromResolver
				})
				.ToList();

			if (isFromResolver || !effects.Any(x => x.EffectData.IsManualPick))
			{
				Execute(owner, picksInfoList);
				return;
			}

			//ManualPick target
			Action onPicked = () =>
			{
				CommandController.Enqueue(() =>
				{
					GameBus.OnSpawnConfirmed += owner.DataBase;
					Execute(owner, picksInfoList);
					GameBus.OnPickedConfirmed += true;
				});
			};

			Action onCanceled = () => GameBus.OnSpawnCanceled += owner.DataBase;

			// Apply and cancellation ability from the summon
			if (owner.DataBase.IsSpawnedByEffect)
			{
				GameBus.OnSpawnConfirmed += owner.DataBase;
				onPicked = () => CommandController.Enqueue(() => Execute(owner, picksInfoList));
				onCanceled = () =>
				{
					picksInfoList.ForEach(x => x.EffectState.EffectData.TargetMod = EffectTargetMod.Self);
					onPicked.Invoke();
				};
				GameBus.OnPickedConfirmed += true;
			}

			TargetResolver.GetManualTarget(picksInfoList, onPicked, onCanceled);
		}

		private static void Execute(IInteractiveEntity owner, IEnumerable<PickInfo> picksInfoList)
		{
			picksInfoList.ForEach(pickInfo =>
			{
				if (pickInfo.Targets == null)
					throw new InvalidOperationException(
						"EffectHandler : PickInfo does not exist any targets but trying to preform\n" +
						$"Effect : {pickInfo.EffectState.EffectData.Effect}\n," +
						$"Id : {pickInfo.EffectState.EffectData.Id}");

				if (!pickInfo.IsFromResolver && !pickInfo.Targets.Any())
					TargetResolver.AutoAssignTargets(pickInfo);

				pickInfo.ToBehaviour().Perform(pickInfo.Targets);
				if (pickInfo.EffectState.EffectData.EndPhase == EffectEndPhase.Use)
					pickInfo.ToBehaviour().StepForward();

				var action = pickInfo.EffectState.EffectData.Effect.GetFamily() == EffectFamily.Summon
					? ActionType.Summon
					: ActionType.Effect;

				if (!pickInfo.IsFromResolver)
					BatchController.PlayerPerformAction(owner, pickInfo.Targets, action,
						pickInfo.EffectState.EffectData.Phase, pickInfo.EffectState.EffectData.Effect, false);
				RRLogger.Log(
					$"[{"EffectHandler".Blue().Bold()}] Execute effect {pickInfo.EffectState.EffectData.Effect}\n" +
					$"From: {pickInfo.From.DataBase.Title}, {pickInfo.From.DataBase.Id}, {pickInfo.From.DataBase.UID.Green()}\n" +
					$"To: {string.Join("\n", pickInfo.Targets.Select(x => $"{x?.DataBase.Title}, {x?.DataBase.Id}, {x?.DataBase.UID.Green()}"))}");
			});
		}

		//Called after Entities' OnNextRound
		public static void HandleRound(RoundData round)
		{
			if (!IsSelfOrBot())
				return;

			//On Game Start
			if (round.RoundNumber == 1
			    && ActorsContextResolver.Self.PlayerIndex == 0)
				GameBus.LocalContext.Vulcanites.ForEach(owner => Handle(owner, EffectPhase.OnGameStart));

			var ownerEntities = GameBus.LocalContext.GetOwnerEntities(round.TurnOwner);

			//On First Turn
			if (round.RoundNumber == 1 || round.RoundNumber == 2)
				ownerEntities.ForEach(owner =>
					Handle(owner, EffectPhase.OnFirstTurn)); // todo check if all effects decrease length by 1

			//Before Next Round
			ownerEntities.ForEach(owner =>
				Handle(owner, EffectPhase.BeforeNextRound)); // todo check if all effects decrease length by 1

			//StepOver all effects
			ownerEntities.ForEach(x => StepForward(x, EffectEndPhase.StartRound));

			bool IsSelfOrBot()
			{
				return GameBus.CurrentRound.Value.TurnOwner == Owner.Self
				       || ActorsContextResolver.Opponent.IsControlledByAI
				       || ActorsContextResolver.Opponent.IsControlledOnClient();
			}
		}

		public static void StepForward(IInteractiveEntity owner, EffectEndPhase effectEndPhase)
		{
			if (owner == null)
			{
				RRLogger.Error("StepForward cannot be executed because IInteractiveEntity owner is missing".Red());
				return;
			}

			var effectStates = owner.DataBase.EffectsContainer.Values.ToArray(); // required to modify the collection
			effectStates
				.Where(state => state.EffectData.EndPhase == effectEndPhase)
				.ForEach(state =>
				{
					var pickInfo = new PickInfo(state, owner);
					if (pickInfo.From == null)
					{
						RRLogger.Error(
							"Phase effect cannot be executed because there are no references in PickInfo".Red());
						return;
					}

					if (pickInfo.ToBehaviour().StepForward())
						BatchController.ModifySelfEntity(pickInfo.From);
				});
		}

		[SerializeField, ReadOnly] public string EffectTypes = "test";

		private void Awake()
		{
			// webgl
			ReCreateEffectTypesRuntime();
		}

		private void ReCreateEffectTypesRuntime()
		{
			var types = JsonConvert.DeserializeObject<Dictionary<EffectKeyword, Type>>(EffectTypes);
			Effects = new Dictionary<EffectKeyword, IEffectBehaviour>();
			foreach (var beh in types) Effects.Add(beh.Key, (IEffectBehaviour)Activator.CreateInstance(beh.Value));
		}

#if UNITY_EDITOR

		private void OnValidate()
		{
			EffectTypes = JsonConvert.SerializeObject(CreateEffectBehaviours());
		}

		private static Dictionary<EffectKeyword, Type> CreateEffectBehaviours()
		{
			return TypeCache.GetTypesWithAttribute<EffectAttribute>()
				.ToDictionary(y => y.GetCustomAttribute<EffectAttribute>().Effect, x => x);
		}

#endif
	}
}