using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Berserk.Shared.Data.Enums;
using Events;
using Game.Entities;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using Vulcan.Data;
using Vulcan.Network.Context;
using Vulcan.Network.Resolver;
using CardData = Vulcan.Data.CardData;
using EffectPhase = Vulcan.Data.EffectPhase;

namespace Vulcan.Network
{
	public class BatchController
	{
		public static event Action<bool> OnMuteStateChanged;
		
		private static bool isMuted;
		private static Task sendBatches;
		private static readonly Queue<Batch> BATCHES = new Queue<Batch>();
		
		public static bool IsRunning { get; private set; }
		public static bool IsEmpty => BATCHES.Count == 0;
		public static bool IsMuted
		{
			get => isMuted;
			private set
			{
				isMuted = value;
				OnMuteStateChanged?.Invoke(isMuted);
			}
		}


		public static async Task SyncAsync()
		{
			if (IsRunning)
				return;

			while (BATCHES.Count > 0)
			{
				IsRunning = true;
				if (sendBatches != null)
					return;

				var batches = new List<Batch>();
				while (BATCHES.Count > 0)
				{
					var batch = BATCHES.Dequeue();
					if (batch.MessageType == MessageType.NextRound 
					    && batch.Data is RoundData fromRound 
					    && !CanSendRound(fromRound))
					{
						continue;
					}
					batches.Add(batch);
				}

				RRLogger.Log($"[{"Batcher".Purple().Bold()}]: Try Sync {nameof(BATCHES)}");
				sendBatches = CardBatcher.SendRequestAsync(batches);
				await sendBatches.ConfigureAwait(true);

				sendBatches = null;
			}

			IsRunning = false;
		}

		public static void Mute()
		{
			IsMuted = true;
			RRLogger.Log($"[{"Batcher".Purple().Bold()}]: is muted");
		}

		public static void Unmute()
		{
			IsMuted = false;
			RRLogger.Log($"[{"Batcher".Purple().Bold()}]: is unmuted");
		}

		public static void Clear()
		{
			BATCHES.Clear();
			sendBatches = null;
			IsRunning = false;
			RRLogger.Log($"[{"Batcher".Purple().Bold()}]: {nameof(Queue<Batch>)} BATCHES is clear");
		}

		public static void NextRound(RoundData from)
		{
			if (IsMuted)
				return;

			if (BATCHES.Any(x => x.MessageType == MessageType.NextRound))
				return;

			var sessionPlayer = ActorsContextResolver.GetPlayer(from.TurnOwner);
			BATCHES.Enqueue(new Batch(from, MessageType.NextRound, sessionPlayer.UserName));
			RRLogger.Log($"[{"Batcher".Purple().Bold()}]: Add batch to {nameof(BATCHES)} - {MessageType.NextRound}");
		}
		
		public static bool CanSendRound(RoundData fromRound)
		{
			var fromSignal = ResolverBus.OnRoundChanged.Value;
			if (fromSignal != null && fromSignal.RoundModel.RoundNumber != fromRound.RoundNumber)
			{
				RRLogger.Error($"[{"BatchController.CanSendRound".Red().Bold()}] " +
				                 $"Round send interrupted because round alredy changed,\n" +
				                 $"old round : {fromRound}\n" +
				                 $"new round : {fromSignal.ToRoundData()}");
				return false;
			}

			return true;
		}

		/// <summary>
		/// Add card to hand
		/// </summary>
		public static async void AddCards(params CardData[] data)
		{
			if (IsMuted || ActorsContextResolver.IsNotSelfAndNotBot())
				return;

			if (data.Any(cardData => string.IsNullOrEmpty(cardData.UID)))
			{
				RRLogger.Error($"[{"Batcher".Purple().Bold()}]: Has empty UID. Batch data: " +
				               $"[{string.Join("\n", data.Select(x => x.Owner + "'s " + x.Title).ToArray())}]");
				return;
			}

			var batch = CardBatcher.PrepareBatch(MessageType.AddCard, GameBus.CurrentRound.Value.TurnOwner, data.Length, data);
			if (batch == null)
				return;

			BATCHES.Enqueue(batch);
			RRLogger.Log($"[{"Batcher".Purple().Bold()}]: Add batch to {nameof(BATCHES)} - {batch.MessageType}");
			await SyncAsync();
		}

		/// <summary>
		/// Request card to hand
		/// </summary>
		public static async void RequestCards(int count)
		{
			if (IsMuted || ActorsContextResolver.IsNotSelfAndNotBot())
				return;

			var batch = CardBatcher.PrepareBatch(MessageType.AddCard, GameBus.CurrentRound.Value.TurnOwner, count);
			if (batch == null)
				return;

			BATCHES.Enqueue(batch);
			RRLogger.Log($"[{"Batcher".Purple().Bold()}]: Add batch to {nameof(BATCHES)} - {batch.MessageType}");
			await SyncAsync();
		}

		/// <summary>
		/// Add card to board
		/// </summary>
		/// <param name="data"></param>
		public static void PlayCards(params CardData[] data)
		{
			if (IsMuted || ActorsContextResolver.IsNotSelfAndNotBot())
				return;
			
			if (data.Any(cardData => string.IsNullOrEmpty(cardData.UID)))
			{
				RRLogger.Error(
					$"[{"Batcher".Purple().Bold()}]: Has empty UID. Batch data:  [{string.Join("\n", data.Select(x => x.Owner + "'s " + x.Title).ToArray())}]");
				return;
			}

			var owner = GameBus.CurrentRound.Value.TurnOwner;
			var batch = CardBatcher.PrepareBatch(MessageType.PlayCard, owner, data.Length, data);
			if (batch == null)
				return;

			BATCHES.Enqueue(batch);
			RRLogger.Log($"[{"Batcher".Purple().Bold()}]: Add batch to {nameof(BATCHES)} - {batch.MessageType}");
		}

		/// <summary>
		///     Perform remote action (attack or effect) from signal
		/// </summary>
		/// <param name="mutableEntities"> who changes </param>
		/// <param name="sourceModifying"> who init change </param>
		/// <param name="attackType"> single attack or effect </param>
		/// <param name="phase"> default = None </param>
		/// <param name="effect"></param>
		/// <param name="defenceDamage">Can an attacker get responce damage</param>
		public static void PlayerPerformAction(IInteractiveEntity sourceModifying,
		                                       IInteractiveEntity[] mutableEntities,
		                                       ActionType attackType,
		                                       EffectPhase phase = EffectPhase.None,
		                                       EffectKeyword effect = EffectKeyword.None,
		                                       bool defenceDamage = true)
		{
			if (IsMuted || ActorsContextResolver.IsNotSelfAndNotBot())
				return;

			if (!mutableEntities.All(Check))
				return;

			var batch = PlayerPerformActionBatcher.PrepareBatch(sourceModifying, mutableEntities, attackType, phase, effect, defenceDamage);
			BATCHES.Enqueue(batch);
			RRLogger.Log($"[{"Batcher".Purple().Bold()}]: Add batch to {nameof(BATCHES)} - {batch.MessageType}");
		}

		public static void ModifySelfEntity(IInteractiveEntity mutableEntity)
		{
			if (IsMuted)
				return;

			if (ActorsContextResolver.IsNotSelfAndNotBot())
				return;

			if (!Check(mutableEntity))
				return;

			var batch = ModifyRequestBatcher.PrepareBatch(mutableEntity);
			BATCHES.Enqueue(batch);
			RRLogger.Log($"[{"Batcher".Purple().Bold()}]: Add batch to {nameof(BATCHES)} - {batch.MessageType}");
		}
		
		private static bool Check(IInteractiveEntity mutableEntity)
		{
			return mutableEntity != null;
		}
	}
}
