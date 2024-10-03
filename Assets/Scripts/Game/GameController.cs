using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Berserk.Shared.Data.Enums;
using BerserkV3.Common.SceneService;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.Network;
using Events;
using Game.Effect_System;
using Game.Timer;
using Lobby;
using RR.Core.DebugSystem;
using RR.Core.EventLayer;
using RR.Core.Extensions;
using UI;
using UnityEngine;
using Vulcan.Data;
using Vulcan.Network;
using Vulcan.Network.Context;
using Vulcan.Network.Resolver;
using EffectPhase = Vulcan.Data.EffectPhase;
using Scene = BerserkV3.Common.SceneService.Scene;

namespace Game
{
	public class GameController : MonoBehaviour
	{
		[SerializeField] private RoundTimer roundTimer;
		private bool reconnection;

		private void Awake()
		{
			// GameBus.OnLoading += true;
			// reconnection = false;
			// GameBus.CurrentRound.SubscribeFirst(this, data => HandleNextRound(data).ConfigureAwait(true));
			// GameBus.OnGameLeft.Subscribe(this, () => LeaveGame(Scene.Lobby));
			// GameBus.OnReconnectRequired.Subscribe(this, RequestReconnect);
			// ResolverBus.OnSessionEnd.Subscribe(this, ResolveSessionEnd);
			// ResolverBus.OnSurrender.Subscribe(this, ResolveSurrender);
			// ResolverBus.OnBanned.Subscribe(this, ResolveBanned);
		}

		private void Start()
		{
			// RRLogger.Log(
			// 	$"[{"Logic".Green().Bold()}] {"__________________GAME START_______________________".Green().Bold()}");
			BatchController.Mute();
		}

		#region NextRoundHandlers

		private async Task HandleNextRound(RoundData roundData)
		{
			if (GameBus.LocalContext.RoundAccepted ||
			    ActorsContextResolver.IsTurnOwnerNotSelfAndNotBot(roundData.TurnOwner))
			{
				RRLogger.Log($"[{"GameController.HandleNextRound".Orange().Bold()}] Not handled reson :\n" +
				             $"IsNotSelfAndNotBot : {ActorsContextResolver.IsTurnOwnerNotSelfAndNotBot(roundData.TurnOwner)}\n" +
				             $"RoundAccepted : {GameBus.LocalContext.RoundAccepted},\n" +
				             $"RoundData : {roundData}");
				return;
			}

			// await context, to add batches to BatchController
			await SafeTask.Await(() => !BatchController.IsMuted).ConfigureAwait(true);

			CommandController.Enqueue(() =>
			{
				HandleOpponentEndRound(roundData);
				HandleLoseCondition(roundData);
				HandleActorsRound(roundData);
				EffectHandler.HandleRound(roundData);
				HandleHandCardsRound(roundData);
				HandleTableCardsRound(roundData);
				CommandController.Enqueue(() => HandleRoundAcceptedAsync().ConfigureAwait(false));
				HandleDeckExhausted(roundData);
			});
		}

		private static void HandleOpponentEndRound(RoundData roundData)
		{
			CommandController.Enqueue(() =>
			{
				var handleOwner = roundData.TurnOwner.GetOppositeOwner();
				var ownerEntities = GameBus.LocalContext.GetOwnerEntities(handleOwner);
				ownerEntities.ForEach(owner =>
				{
					owner.IsCanAttack = false;
					BatchController.ModifySelfEntity(owner);
				});
				ownerEntities.ForEach(owner => EffectHandler.Handle(owner, EffectPhase.OnBeforeEndRound));
				ownerEntities.ForEach(owner => EffectHandler.StepForward(owner, EffectEndPhase.EndRound));
				RRLogger.Log(
					$"[{"NextRoundResolver.HandleOpponentEndRound".Orange().Bold()}] Handle end round : {handleOwner}");
			});
		}

		private void HandleActorsRound(RoundData round)
		{
			var entities = GameBus.LocalContext.Vulcanites.ToArray();
			foreach (var entity in entities)
			{
				var myTurn = round.TurnOwner == entity.Owner;
				entity.IsCanAttack = myTurn;

				if (myTurn)
				{
					var lava = GetLavaByRound(round.RoundNumber);
					entity.Data.Lava.SetMax(lava, true);
					BatchController.ModifySelfEntity(entity);
				}

				entity.SetTurn(myTurn);
			}

			int GetLavaByRound(int roundNumber)
			{
				var lavaRound = 1 * roundNumber;
				var currentLava = roundNumber > 2
					? Mathf.Min(lavaRound / 2 + lavaRound % 2, 10)
					: 1 * roundNumber; //the second round is an exception to the formula

				return currentLava;
			}
		}

		private void HandleTableCardsRound(RoundData round)
		{
			var entities = GameBus.LocalContext.TableCardsList.ToArray();
			foreach (var entity in entities)
			{
				var isEntityTurn = round.TurnOwner == entity.Owner;
				entity.SetTurn(isEntityTurn);

				if (isEntityTurn)
					BatchController.ModifySelfEntity(entity);
			}
		}

		private void HandleHandCardsRound(RoundData round)
		{
			var entities = GameBus.LocalContext.HandCardsList.ToArray();
			foreach (var entity in entities)
			{
				var isEntityTurn = round.TurnOwner == entity.Owner;
				entity.SetTurn(isEntityTurn);
			}
		}

		private void HandleLoseCondition(RoundData roundData)
		{
			var turnOwner = roundData.TurnOwner;
			var localContext = GameBus.LocalContext;

			var entity = localContext.GetVulcaniteByOwner(turnOwner);
			if (entity.Data.Hp <= 0)
			{
				SetLose();
				return;
			}

			// check if deck is empty
			var count = localContext.GetDeckCardCount(turnOwner);
			if (count > 0)
				return;

			// check if hand is empty
			var hand = localContext.GetHandCardsCount(turnOwner);
			if (hand > 0)
				return;

			// check if table is empty
			var table = localContext.GetTableCardsByOwner(turnOwner);
			if (table.Any())
				return;

			SetLose();

			void SetLose()
			{
				// deck, hand and table are empty => auto lose
				entity.KillSelf();
				BatchController.ModifySelfEntity(entity);
			}
		}

		private async Task HandleRoundAcceptedAsync(int retries = 3)
		{
			return;
			// if (await GameAPI.PostRoundAccepted() == HttpStatusCode.OK)
			// {
			// 	GameBus.LocalContext.RoundAccepted = true;
			// 	return;
			// }

			if (GameBus.LocalContext.RoundAccepted)
			{
				RRLogger.Error(
					$"[{"GameController.HandleRoundAcceptedAsync".Red().Bold()}] {"Round acepted but strange!".Green()}");
				return;
			}

			if (retries <= 0)
			{
				RRLogger.Error(
					$"[{"GameController.HandleRoundAcceptedAsync".Red().Bold()}] Round acepted {"Fail".Red().Bold()}");
				GameBus.OnReconnectRequired += true;
				return;
			}

			RRLogger.Error($"[{"GameController.HandleRoundAcceptedAsync".Red().Bold()}] " +
			               $"round cannot acepted, {"Try".Yellow().Bold()} : {3 - retries}");
			HandleRoundAcceptedAsync(retries - 1).GetAwaiter();
		}

		private void HandleDeckExhausted(RoundData roundData)
		{
			if (roundData.TurnOwner == Owner.Self &&
			    GameBus.LocalContext.GetDeckCardCount(roundData.TurnOwner) <= 0)
				GameBus.OnActionBlocked += BlockedInfo.DeckExhausted;
		}

		#endregion

		private void ResolveSurrender(SessionEndMessage message)
		{
			RRLogger.Log(
				$"[{"Logic".Green().Bold()}] {"__________________SURRENDER_______________________".Green().Bold()}");
			GameBus.LocalContext.Vulcanites?.FirstOrDefault(x => x.Data.UID == message.SessionPlayerId)?.KillSelf();
			ResolveSessionEnd(message);
		}

		private void ResolveSessionEnd(SessionEndMessage message)
		{
			StopGame();
			RRLogger.Log(
				$"[{"Logic".Green().Bold()}] {"__________________GAME END_______________________".Green().Bold()}");
		}

		private void ResolveBanned()
		{
			RRLogger.Log(
				$"[{"Logic".Red().Bold()}] {"________________PLAYER BANNED____________________".Red().Bold()}");
			StopGame();
			LobbyBus.LogOut += true;
		}

		private void RequestReconnect()
		{
			if (reconnection || GameBus.LocalContext.GameEnded)
				return;

			reconnection = true;
			StopGame();

			ConfirmationDialog.Instance.Init()
				.SetMessage("There was a connection loss, please retry the connection, press button to continue.")
				.SetAnyResponse(() => LeaveGame(Scene.StartUp))
				.SetTitle("Connection")
				.SetOk("Try again")
				.SetCancel()
				.Apply();
		}

		// Do not close connection here!
		private void StopGame()
		{
			BatchController.Mute();
			GameBus.LocalContext.GameEnded = true;
			GameBus.OnPassingTurned += true;
			RequestsResolver.Clear();
			CommandController.Clear();
			BatchController.Clear();
			EventQueue.RemoveAll();
			roundTimer.StopTimer();
			GameBus.CurrentRound = new State<RoundData>();
		}

		private async void LeaveGame(Scene targetScene)
		{
			if (targetScene != Scene.StartUp)
			{
				if (reconnection)
					return;

				StopGame();
			}

			GameBus.OnBlockUI += true;
			await SceneServiceAdapter.Service.LoadAsync(targetScene).AddLoadingTask();
			GameBus.OnBlockUI += false;
		}

		private void OnDestroy()
		{
			reconnection = false;
			ActorsContextResolver.Dispose();
			// DeckManager.Dispose();
			GameBus.ResetStates();
			ResolverBus.ResetStates();
		}
	}
}