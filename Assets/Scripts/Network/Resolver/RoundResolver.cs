using System;
using System.Collections;
using System.Threading.Tasks;
using Berserk.Shared.Data.Enums;
using Events;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using ServerCore.Infrastructure.Models;
using UnityEngine;
using Vulcan.Data;
using Vulcan.Network.Context;

namespace Vulcan.Network.Resolver
{
	public class RoundResolver : MonoBehaviour
	{
		private void Start()
		{
			GameBus.OnVulcaniteDies.Subscribe(this, () =>
			{
				GameBus.LocalContext.GameEnded = true;
				GameBus.OnPassingTurned += true;
			});
			GameBus.OnRoundEnd.Subscribe(this, FinishRound);
		}
		
		public static void Resolve(RoundMessage roundMessage, long timestamp)
		{
			EventQueue.Enqueue(new GameEvent(GameEventType.NextRound, NextRoundCoroutine(), timestamp));

			IEnumerator NextRoundCoroutine()
			{
				GameBus.LocalContext.State = SessionState.Battle;
				GameBus.LocalContext.RoundAccepted = false;
				GameBus.OnPassingTurned += true;
				
				var updateTask = UpdateRoundAsync(roundMessage.ToRoundData(), true);
				yield return new WaitUntil(() => updateTask.IsCompleted);
			}
		}

		/// <param name="data">new round data</param>
		/// <param name="delayed">Betwen rounds need delay but, no need to another states</param>
		public static async Task UpdateRoundAsync(RoundData data, bool delayed = false)
		{
			RRLogger.Log($"UpdateRoundAsync : {data}");
			if (data.TurnOwner == Owner.None || data.RoundNumber <= 0)
				return;

			//delay for playing all animations. 
			if(delayed)
				await Task.Delay(TimeSpan.FromSeconds(2f)).ConfigureAwait(true); // TODO: Remove when transferring game logic to the server
			
			ChangeRoundAfterAwaitContext(data); // !!! Important, don't await, has a circular dependency.
		}

		private async void FinishRound()
		{
			if (ActorsContextResolver.IsNotSelfAndNotBot())
				return;
			var fromRound = GameBus.CurrentRound.Value; // !!! Importans, capture before await
			await SafeTask.Await(() => !BatchController.IsMuted).ConfigureAwait(true);
			CommandController.Enqueue(() =>
			{
				if (!BatchController.CanSendRound(fromRound))
					return;
				
				BatchController.NextRound(fromRound);
			}, CommandType.NextRound);
			RRLogger.Log($"[{"FinishRound".White().Bold()}]");
		}

		private static async void ChangeRoundAfterAwaitContext(RoundData data)
		{
			await SafeTask.Await(() => !BatchController.IsMuted).ConfigureAwait(true);
			GameBus.CurrentRound += data;
		}
	}
}
