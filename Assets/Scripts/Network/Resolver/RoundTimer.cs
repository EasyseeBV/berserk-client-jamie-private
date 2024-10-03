using System;
using System.Collections;
using Berserk.Shared.Data.Enums;
using BerserkV3.Common.Network;
using BerserkV3.Startup.Network;
using Events;
using RR.UI.DebugSystem;
using ServerCore.Infrastructure.Models;
using UnityEngine;
using Vulcan.Network;
using Vulcan.Network.Resolver;
using Environment = BerserkV3.Startup.Network.Enums.Environment;

namespace Game.Timer
{
	public class RoundTimer : MonoBehaviour
	{
		private bool isTimerPaused;
		private Coroutine timerRoutine;

		private void Start()
		{
			GameBus.OnPassingTurned.Subscribe(this, StopTimer);
			GameBus.OnVulcaniteDies.Subscribe(this, StopTimer);
			GameBus.OnShowMulligan.Subscribe(this, UpdateTimer);
			GameBus.OnContextUpdated.Subscribe(this, UpdateTimer);
			GameBus.OpponentDisconnected.Subscribe(this, isDisconnected => !isDisconnected, UpdateTimer);
			GameBus.CurrentRound.Subscribe(this, round =>
				{
					GameBus.LocalContext.SetTurnTimerEnd(round.TurnTimerEndsOn);
					UpdateTimer();
				}
			);
			
			ResolverBus.OnTimerPaused.Subscribe(this, message =>
				{
					Pause(message.EntDateTime);
					UpdateTimer();
				}
			);
			ResolverBus.OnTimerResumed.Subscribe(this, message =>
				{
					Unpause(message.EntDateTime);
					UpdateTimer();
				}
			);

			if (EnvironmentSwitcher.CurrentEnvironment <= Environment.Staging)
				RRConsole.AddCommand(nameof(PauseTimer), PauseTimer, "Pause/Unpause the round timer.");
		}

		public void StopTimer()
		{
			if (timerRoutine != null)
			{
				StopCoroutine(timerRoutine);
				timerRoutine = null;
			}
		}

		private void UpdateTimer()
		{
			StopTimer();

			if (GameBus.OpponentDisconnected)
			{
				GameBus.OnTimerStateUpdated += true;
				return;
			}

			if (GameBus.LocalContext.State != SessionState.Mulligan
			    && GameBus.CurrentRound.Value.TurnOwner == Owner.None)
				return;

			timerRoutine = StartCoroutine(RoundTimerRoutine(GameBus.LocalContext.TurnTimerEndsOn));

			GameBus.OnTimerStateUpdated += true;
		}

		private void Pause(DateTime endTime)
		{
			GameBus.LocalContext.SetTurnTimerEnd(endTime);
			StopTimer();
			isTimerPaused = true;
			GameBus.OnTimerPaused += isTimerPaused;
		}

		private void Unpause(DateTime endTime)
		{
			GameBus.LocalContext.SetTurnTimerEnd(endTime);
			isTimerPaused = false;
			GameBus.OnTimerPaused += isTimerPaused;
		}
		
		private IEnumerator RoundTimerRoutine(DateTime endTime)
		{
			TimerTick(true);
			
			var tick = new WaitForSeconds(.99f);
			var pause = new WaitWhile(() => isTimerPaused);

			while (true)
			{
				yield return pause;

				TimerTick();

				yield return tick;

				if (GameBus.RoundTimer <= 0)
					break;
			}

			yield return tick;

			GameBus.RoundTimer += 0;
			GameBus.OnPassingTurned += true;

			void TimerTick(bool forceAssign = false)
			{
				var timer = endTime - DateTime.UtcNow;
				var roundTimer = Math.Max(0, (int) timer.TotalSeconds);

				if (forceAssign)
					GameBus.RoundTimer.Assign(roundTimer);
				else
					GameBus.RoundTimer += roundTimer;
			}
		}

		private string PauseTimer(string[] args)
		{
			isTimerPaused = !isTimerPaused;
			return isTimerPaused ? "Paused!" : "Unpaused!";
		}
	}
}