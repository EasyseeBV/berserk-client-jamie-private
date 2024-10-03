using System.IO;
using System.Threading.Tasks;
using BerserkV3.Startup.Events;
using Events;
using Newtonsoft.Json;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using ServerCore.Infrastructure.Models;
using UnityEngine;
using Vulcan.Data;
using Vulcan.Network.Resolver;

namespace Vulcan.Network.Context
{
	public class ContextResolver : MonoBehaviour
	{
		private void Awake()
		{
			gameObject.GetOrAddComponent<RequestsResolver>();
		}

		private void Start()
		{
			GameBus.FetchContext.Subscribe(this,
				value => GameBus.FetchContext.PrevValue == false,
				() => FetchAsync().ConfigureAwait(false));
		}

		private async Task FetchAsync()
		{
			// if (await ValidateClientEnvironment() == false)
			// 	return;
			//
			// GameBus.FetchContext.Assign(true);
			// RRLogger.Log(
			// 	$"[{nameof(ContextResolver).Red().Bold()}] [{"_________________UPDATE CONTEXT_________________".Red().Bold()}]");
			// BatchController.Clear();
			// RequestsResolver.Clear();
			// CommandController.Clear();
			// CommandController.Restart();
			//
			// var context = await GameAPI.GetContext();
			//
			// if (await ValidateContext(context) == false)
			// 	return;
			//
			// if (EnvironmentSwitcher.CurrentEnvironment <= Environment.Staging)
			// 	WriteToLocalStorage(context, "context");
			//
			// // var selfIndex = context.Players?.FirstOrDefault(p => p.UserName == User.UserName)?.PlayerIndex ?? 0;
			// // GameBus.SessionRNG = new Random(context.StartDateTime.GetHashCode() + selfIndex);
			//
			// try
			// {
			// 	await ResolveAsync(context);
			// }
			// catch (Exception exception)
			// {
			// 	ErrorDispatcher.OnException += exception;
			// 	return;
			// }
			//
			// //await GameHub.ResolveBufferingAsync(context.UtcTimeStamp).ConfigureAwait(true);
			// // update players status after all resolvers
			// ActorsContextResolver.UpdatePlayersStatus();
			//
			// // check state after resolve
			// if (context.State == SessionState.Mulligan)
			// 	GameBus.OnShowMulligan += true;
			//
			// GameBus.UpdateGraveyard += true;
			//
			// EndFetchContext();
			// GameBus.OnContextUpdated += true;
		}

		private async Task ResolveAsync(SessionContextModel remoteContext)
		{
			if (await ValidateClientEnvironment() == false)
				return;

			EventQueue.RemoveAllOlderThan(remoteContext.UtcTimeStamp);

			GameBus.LocalContext.SessionId = remoteContext.SessionId;
			GameBus.LocalContext.StartDateTime = remoteContext.StartDateTime!.Value;
			GameBus.LocalContext.RoundNumber = remoteContext.RoundNumber;
			GameBus.LocalContext.RoundAccepted = remoteContext.RoundAccepted;
			GameBus.LocalContext.TimeStamp = remoteContext.UtcTimeStamp;
			GameBus.LocalContext.SetTurnTimerEnd(remoteContext.TurnTimerEndsOn);
			GameBus.LocalContext.UseLeaderboardStatistics = remoteContext.UseLeaderboardStatistics;
			GameBus.LocalContext.State = remoteContext.State;

			GameBus.CurrentRound.Assign(remoteContext.ToRoundData());
			ActorsContextResolver.Resolve(remoteContext.Players);

			await RoundResolver.UpdateRoundAsync(GameBus.CurrentRound);
			// CustomisationContextResolver.Resolve();
		}

		private Task<bool> ValidateClientEnvironment()
		{
			if (GameBus.LocalContext.GameEnded)
			{
				EndFetchContext();
				RRLogger.Error($"[{nameof(ContextResolver).Red().Bold()}] Fetch context failed, game ended!");
				return Task.FromResult(false);
			}

			if (StartupBus.IsAuthorizing)
			{
				EndFetchContext();
				RRLogger.Log($"[{nameof(ContextResolver).Red().Bold()}] [{"Fetch context failed, StartupBus.IsAuthorizing".Red().Bold()}]");
				return Task.FromResult(false);
			}

			if (GameBus.OnReconnectRequired)
			{
				EndFetchContext();
				RRLogger.Error($"[{nameof(ContextResolver).Red().Bold()}] Fetch context failed, OnReconnectRequired");
				return Task.FromResult(false);
			}

			return Task.FromResult(true);
		}

		private async Task<bool> ValidateContext(SessionContextModel context)
		{
			if (await ValidateClientEnvironment() == false)
				return false;

			if (context == null)
			{
				EndFetchContext();
				RRLogger.Error($"[{nameof(ContextResolver).Red().Bold()}] Fetch context failed, context is null");
				return false;
			}

			if (!context.StartDateTime.HasValue)
			{
				EndFetchContext();
				RRLogger.Error($"[{nameof(ContextResolver).Red().Bold()}] Fetch context failed, session not started");
				return false;
			}

			if (context.EndDateTime.HasValue)
			{
				EndFetchContext();
				RRLogger.Error($"[{nameof(ContextResolver).Red().Bold()}] Fetch context failed, session already closed");
				return false;
			}

			if (context.Players == null || context.Players.Count < 2)
			{
				EndFetchContext();
				RRLogger.Error($"[{nameof(ContextResolver).Red().Bold()}] Fetch context failed, session with too low count of players");
				return false;
			}

			return true;
		}

		private void EndFetchContext()
		{

		}

		private void WriteToLocalStorage(object value, string fileName)
		{
			var p = Path.Combine(UnityEngine.Application.persistentDataPath, $"{fileName}.txt");
			File.WriteAllText(p, JsonConvert.SerializeObject(value));
		}
	}
}