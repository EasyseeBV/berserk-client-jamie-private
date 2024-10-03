using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Events;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using UnityEngine;
using Vulcan.Network.Resolver;

namespace Vulcan.Network
{
	public class CommandController : MonoBehaviour, IDisposable
	{
		private static readonly float ACTION_DELAY = 0.5f;
		private static readonly Queue<Command> COMMANDS = new Queue<Command>();

		private static CommandController instance;
		private Coroutine commandLoopCoroutine;
		private readonly WaitForSeconds waitAction = new WaitForSeconds(ACTION_DELAY);
		private readonly WaitWhile waitRoundAccepted = new WaitWhile(() => !GameBus.LocalContext.RoundAccepted);
		private readonly WaitWhile waitSyncActions = new WaitWhile(() => BatchController.IsRunning);
		private readonly WaitWhile waitCallbackActionSignal = new WaitWhile(() => !RequestsResolver.WaitListIsEmpty);

		private void Awake()
		{
			if (instance == null)
				instance = this;
			else if (instance != this)
				Destroy(gameObject);

			StartCoroutine(SafeLoopCoroutine());
		}

		public static void Enqueue(Action action, CommandType commandType = CommandType.Action)
		{
			if (COMMANDS.Any(x => commandType == CommandType.NextRound && x.CommandType == commandType))
				return;

			var command = new Command(action, commandType);

			COMMANDS.Enqueue(command);
			RRLogger.Log($"[{"Command".Yellow().Bold()}] {nameof(Enqueue)} new command {commandType}, action - {action.Method.Name}");

			instance.commandLoopCoroutine ??= instance.StartCoroutine(instance.CommandLoopCoroutine());
		}

		public static void Restart()
		{
			instance.StartCoroutine(instance.SafeLoopCoroutine());
		}

		public static void Clear()
		{
			instance.Dispose();
		}

		private IEnumerator CommandLoopCoroutine()
		{
			while (COMMANDS.Count > 0)
			{
				if (COMMANDS.Count > 1 && COMMANDS.Peek().CommandType == CommandType.NextRound)
					COMMANDS.Enqueue(COMMANDS.Dequeue());

				var command = COMMANDS.Dequeue();

				if (command.CommandType == CommandType.NextRound)
				{
					//await reconnection routines
					if (GameBus.LocalContext.RoundNumber > 0)
						yield return waitRoundAccepted;

					yield return waitSyncActions;
					yield return waitCallbackActionSignal;
				}


				command.Perform();
				yield return waitAction; // avoiding concurrent calls
				yield return waitAction; // preventing an empty list of batches 

				BatchController.SyncAsync().ConfigureAwait(true);
				RRLogger.Log($"[{"Command".Yellow().Bold()}] Try {nameof(BatchController.SyncAsync)} command - {command.CommandType}");

				yield return waitSyncActions;
				RRLogger.Log($"[{"Command".Yellow().Bold()}] Waiting end {nameof(waitSyncActions)} command - {command.CommandType}");
			}

			commandLoopCoroutine = null;
		}

		private IEnumerator SafeLoopCoroutine()
		{
			var wait = new WaitForSeconds(2f);
			while (UnityEngine.Application.isPlaying)
			{
				yield return wait;
				if (COMMANDS.Count > 0)
				{
					instance.commandLoopCoroutine ??= instance.StartCoroutine(instance.CommandLoopCoroutine());
					continue;
				}

				if (BatchController.IsEmpty)
					continue;

				RRLogger.Log($"[{"Command".Yellow().Bold()}] Forced sync batches without command.");
				yield return BatchController.SyncAsync();
			}
		}

		public void Dispose()
		{
			instance.StopAllCoroutines();
			COMMANDS.Clear();
		}
	}
}