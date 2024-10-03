using System;
using System.Collections;
using System.Collections.Generic;
using Events;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using ServerCore.Infrastructure.Models;
using UnityEngine;
using Vulcan.Data;
using Vulcan.Network.Context;

namespace Vulcan.Network.Resolver
{
	public class RequestsResolver : MonoBehaviour
	{
		private static readonly List<MessageType> RESPONSE_WAIT_LIST = new List<MessageType>();

		public static bool WaitListIsEmpty => RESPONSE_WAIT_LIST.Count == 0;

		private static int waitRetries;

		private void Start()
		{
			ResolverBus.OnUserStateChanged.Subscribe(this,
			   m => ActorsContextResolver.UpdatePlayerStatus(m.PlayerModel.Id,
			    m.PlayerModel.ConnectionStatus));

			ResolverBus.OnCardPlayed.Subscribe(this,
			    m => ResolveSignal(MessageType.PlayCard, 
			        () => PlayCardResolver.Resolve(m.PlayCardModel, m.UtcTimeStamp), m));

			ResolverBus.OnHandCardAdded.Subscribe(this,
				m => ResolveSignal(MessageType.AddCard,
					() => AddCardResolver.Resolve(m, m.UtcTimeStamp), m));

			ResolverBus.OnEntityModified.Subscribe(this,
				m => ResolveSignal(MessageType.Modify,
					() => ModifyEntityResolver.Resolve(m.ModifyEntityModel.EntityStates.ToArray(), m.UtcTimeStamp), m));

			ResolverBus.OnPlayerAction.Subscribe(this,
				m => ResolveSignal(MessageType.PerformAction,
					() => PerformActionResolver.Resolve(m.EntityStates, m.UtcTimeStamp), m));

			ResolverBus.OnRoundChanged.Subscribe(this,
				m => ResolveSignal(MessageType.NextRound,
					() => RoundResolver.Resolve(m, m.UtcTimeStamp), m));

			StartCoroutine(CheckRequests());
		}

		public static void AddToPending(MessageType type)
		{
			RESPONSE_WAIT_LIST.Add(type);
			RRLogger.Log($"[{"Resolver".Orange().Bold()}] Add To Pending signals - {type}");
		}

		public static void Clear()
		{
			var log = $"[{"Resolver".Orange().Bold()}] Clear responsed signals - {RESPONSE_WAIT_LIST.Count}";
			RESPONSE_WAIT_LIST.ForEach(r => log = $"{log.Red()} <{r}>");
			RRLogger.Log(log);
			RESPONSE_WAIT_LIST.Clear();
			waitRetries = 0;
		}

		private static IEnumerator CheckRequests()
		{
			var waitCheck = new WaitForSeconds(4f);
			waitRetries = 0;

			while (true)
			{
				yield return waitCheck;

				if (WaitListIsEmpty)
				{
					waitRetries = 0;
					continue;
				}

				waitRetries++;

				var log = $"[{"Resolver".Orange().Bold()}] Retry#{waitRetries} Waiting for signals:";
				RESPONSE_WAIT_LIST.ForEach(r => log = $"{log.Red()} <{r}>");
				RRLogger.Log(log);

				if (!WaitListIsEmpty && waitRetries >= 3)
				{
					RRLogger.Error($"[{"CheckRequests".Red().Bold()}] Exhausted check attempts\n {string.Join(";", RESPONSE_WAIT_LIST)}");
					RESPONSE_WAIT_LIST.Clear();
					waitRetries = 0;
					GameBus.OnReconnectRequired += true;
					yield break;
				}
			}
		}

		private static void ResolveSignal(MessageType type, Action signalAction, GameMessage signal)
		{
			if (string.IsNullOrEmpty(signal.SessionPlayerId) || SignalFromNone(signal) )
			{
				RRLogger.Error($"[{"Resolve Signal".Red().Bold()}] {signal.Action}: " + 
				                 $"SignalFromNone SignalOwner = {"None".Red().Bold()}");
				return;
			}

			RRLogger.Log($"[{"Resolve Signal".Orange().Bold()}] {signal.Action}: " +
			             $"SignalOwner = {ActorsContextResolver.GetPlayer(signal.SessionPlayerId)?.UserName ?? "Null".Red().Bold()}");


			if (SignalFromSelf(signal) || SignalFromBot(signal))
				SignalReceived();
			
			if(SignalFromOpponent(signal) && !SignalFromBot(signal))
				ActorsContextResolver.UpdatePlayerStatus(ActorsContextResolver.Opponent.Id, 
				                                         PlayerConnectionStatus.Connected); //force setting connection status
			
			if (!IsRequiredInvoke())
				return;

			signalAction?.Invoke();
			RRLogger.Log($"[{"Resolve Signal".Orange().Bold()}] {signal.Action}: Invoke");

			void SignalReceived()
			{
				if (RESPONSE_WAIT_LIST.Contains(type))
					RESPONSE_WAIT_LIST.Remove(type);
				if (WaitListIsEmpty)
					waitRetries = 0;
				RRLogger.Log($"[{"Resolver".Orange().Bold()}] Remove from Pending signals - {type}");
			}

			bool IsRequiredInvoke()
			{
				return type == MessageType.NextRound
				       || type == MessageType.AddCard
				       || !SignalFromSelf(signal);
			}
		}

		public static bool SignalFromSelf(GameMessage signal)
		{
			return ActorsContextResolver.Self?.Id == signal.SessionPlayerId;
		}

		public static bool SignalFromBot(GameMessage signal)
		{
			return signal.SessionPlayerId == ActorsContextResolver.Opponent.Id
			       && ActorsContextResolver.Opponent.IsControlledByAI;
		}

		public static bool SignalFromOpponent(GameMessage signal)
		{
			return signal.SessionPlayerId == ActorsContextResolver.Opponent.Id 
			       && !ActorsContextResolver.Opponent.IsControlledByAI;
		}

		public static bool SignalFromNone(GameMessage signal)
		{
			return !SignalFromSelf(signal)
			       && !SignalFromBot(signal)
			       && !SignalFromOpponent(signal);
		}
	}
}