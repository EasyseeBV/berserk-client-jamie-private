using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Events;
using Game;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using ServerCore.Infrastructure.Models;
using Vulcan.Data;
using Vulcan.Network.Resolver;

namespace Vulcan.Network.Context
{
	public static class ActorsContextResolver
	{
		public static SessionPlayerModel Self => GetPlayer(Owner.Self);
		public static SessionPlayerModel Opponent => GetPlayer(Owner.Opponent);

		private static readonly Dictionary<Owner, SessionPlayerModel> PLAYERS = new Dictionary<Owner, SessionPlayerModel>();

		public static SessionPlayerModel GetPlayer(Owner owner)
		{
			if (PLAYERS.ContainsKey(owner))
				return PLAYERS[owner];

			RRLogger.Log($"[{nameof(ActorsContextResolver).Red().Bold()}] - Player with owner : {owner} does not represent in collection");
			return null;
		}

		public static IEnumerable<SessionPlayerModel> GetPlayers()
		{
			return PLAYERS.Values;
		}

		public static SessionPlayerModel GetPlayer(string id)
		{
			return PLAYERS.Values.FirstOrDefault(x => x != null && x.Id == id);
		}

		public static Owner GetOwnerByTurn(int roundNumber)
		{
			return GetOwnerByTurn(roundNumber, Self.PlayerIndex);
		}
		
		public static Owner GetOwnerByTurn(int roundNumber, int playerIndex)
		{
			if (roundNumber == 0)
				return Owner.None;
			
			return roundNumber % 2 != playerIndex 
				? Owner.Self 
				: Owner.Opponent;
		}

		public static Owner GetOwnerByName(string name)
		{
			return PLAYERS.FirstOrDefault(x => x.Value != null && x.Value.UserName == name).Key;
		}

		public static Owner GetOwnerById(string id)
		{
			return PLAYERS.FirstOrDefault(x => x.Value != null && x.Value.Id == id).Key;
		}

		public static bool IsPracticeMode => Opponent != null && Opponent.IsControlledByAI && Opponent.UserName == "BotPlayer";

		public static void Resolve(HashSet<SessionPlayerModel> remote)
		{
			PLAYERS.Clear();
			PLAYERS.Add(Owner.Opponent, remote.FirstOrDefault(p => !ResolverHelpers.IsPlayerSelf(p)));
			PLAYERS.Add(Owner.Self, remote.FirstOrDefault(ResolverHelpers.IsPlayerSelf));

			if (PLAYERS.Values.Any(x => x == null))
				RRLogger.Error($"[{"ContextResolver".Orange().Bold()}] Players are null");

			var self = GameBus.LocalContext.GetVulcaniteByOwner(Owner.Self);
			var opponent = GameBus.LocalContext.GetVulcaniteByOwner(Owner.Opponent);

			self.InitActorFromRemote(Self.ToActorData(Owner.Self));
			opponent.InitActorFromRemote(Opponent.ToActorData(Owner.Opponent));

			//SElF
			if (ResolverHelpers.TryFindEntityOnBoard(Self.VulcaniteModel.Id, out var entitySelf))
				ResolverHelpers.UpdateEntity(entitySelf, Self.VulcaniteModel.EntityState);

			//OPPONENT
			if (ResolverHelpers.TryFindEntityOnBoard(Opponent.VulcaniteModel.Id, out var entityOpponent))
				ResolverHelpers.UpdateEntity(entityOpponent, Opponent.VulcaniteModel.EntityState);

			RRLogger.Log($"[{"ContextResolver".Orange().Bold()}] Resolving bot ends");
		}

		public static void UpdatePlayersStatus()
		{
			GetPlayers().ForEach(x=> UpdatePlayerStatus(x.Id, x.ConnectionStatus));
		}
		
		public static void UpdatePlayerStatus(string playerId, PlayerConnectionStatus status)
		{
			var player = GetPlayer(playerId);
			
			if (player == null)
			{
				RRLogger.Error($"[{"ActorsContextResolver.UpdatePlayerStatus".Red().Bold()}]" +
				               $"The status could not be updated because the Player was not found : {playerId}");
				return;
			}

			if (player.ConnectionStatus == status)
			{
				RRLogger.Warning($"Player {playerId} - {player.UserName} alreade {status}");
			}
			else
			{
				if (player.Id == Self.Id)
				{
					player.ConnectionStatus = status;
					if (!player.IsDisconnected())
						return;

					RRLogger.Error($"[{"ActorsContextResolver.UpdatePlayerStatus".Red().Bold()}]" +
					               $"Reconnect reason : The server terminated the connection with player : {playerId}");
					GameBus.OnReconnectRequired += true;
					return;
				}

				if (player.Id == Opponent.Id && IsPracticeMode)
					return;

				player.ConnectionStatus = status;

				GameBus.OpponentDisconnected += player.IsDisconnected();
			}

			var disconnected = player.IsDisconnected();
			var info = disconnected
				? BlockedInfo.OpponentDisconnect
				: BlockedInfo.OpponentConnect;

			GameBus.OnActionBlocked += info;
		}

		public static bool IsNotSelfAndNotBot()
		{
			return IsTurnOwnerNotSelfAndNotBot(GameBus.CurrentRound.Value.TurnOwner);
		}
		
		public static bool IsTurnOwnerNotSelfAndNotBot(Owner turnOwner)
		{
			return turnOwner != Owner.Self
			       && !Opponent.IsControlledByAI
			       && !Opponent.IsControlledOnClient();
		}

		public static void Dispose()
		{
			PLAYERS.Clear();
		}
	}
}