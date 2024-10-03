using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using UnityEngine;
using Vulcan.Network;

namespace Events
{
	//TODO: Name more specifically. Common name.
	// Class implements all events after context resolution completes after update
	public sealed class EventQueue : MonoBehaviour
	{
		private List<GameEvent> timestampQueue = new List<GameEvent>();

		private Coroutine eventLoopCoroutine;
		private readonly WaitWhile awaitContext = new WaitWhile(() => BatchController.IsMuted);
		private readonly WaitForSeconds wait = new WaitForSeconds(3f);

		private static EventQueue instance;

		private void Awake()
		{
			if (instance == null)
				instance = this;
			else if (instance != this)
				Destroy(gameObject);

			StartCoroutine(SafeLoopCoroutine());
		}

		public static void Enqueue(GameEvent action)
		{
			RRLogger.Log($"[{"Resolver".Orange().Bold()}] — ADD {action.Type}:{action.Timestamp % 1000}");

			instance.timestampQueue.Add(action);
			instance.timestampQueue = instance.timestampQueue.OrderBy(x => x.Timestamp).ToList();

			instance.eventLoopCoroutine ??= instance.StartCoroutine(instance.EventLoopCoroutine());
		}

		public static void RemoveAllOlderThan(long timestamp)
		{
			instance.StopAllCoroutines();
			var count = instance.timestampQueue.RemoveAll(x => x.Timestamp < timestamp);
			instance.eventLoopCoroutine = instance.StartCoroutine(instance.EventLoopCoroutine());
			instance.StartCoroutine(instance.SafeLoopCoroutine());
			RRLogger.Log($"[{"Resolver".Orange().Bold()}] {"WARNING".Red().Bold()} — Removed {count} outdated events!");
		}

		public static void RemoveAll()
		{
			instance.StopAllCoroutines();
			var count = instance.timestampQueue.Count;
			instance.timestampQueue.Clear();
			RRLogger.Log($"[{"Resolver".Orange().Bold()}] {"WARNING".Red().Bold()} — Removed {count} outdated events!");
		}

		public static bool IsEmpty()
		{
			return instance.timestampQueue.Count == 0;
		}

		private IEnumerator EventLoopCoroutine()
		{
			while (timestampQueue.Count > 0)
			{
				yield return awaitContext;

				var currentAction = instance.timestampQueue.First();
				instance.timestampQueue.RemoveAt(0);

				if (currentAction.Type == GameEventType.NextRound)
				{
					//wait until all the signals come
					yield return wait;
					if (timestampQueue.Count > 0)
					{
						instance.timestampQueue = instance.timestampQueue.OrderBy(x => x.Timestamp).ToList();
						instance.timestampQueue.Add(currentAction);
						currentAction = instance.timestampQueue.First();
						instance.timestampQueue.RemoveAt(0);
					}
				}

				if (currentAction.Timestamp >= GameBus.LocalContext.TimeStamp)
				{
					RRLogger.Log($"[{"Resolver".Orange().Bold()}] — PERFORM {currentAction.Type} t- {currentAction.Timestamp % 1000}");
					yield return StartCoroutine(currentAction.Action);
					RRLogger.Log($"[{"Resolver".Orange().Bold()}] — PERFORM END {currentAction.Type} t- {currentAction.Timestamp % 1000}");
				}
				else
				{
					RRLogger.Log(
						$"[{"Resolver".Orange().Bold()}] {"WARNING".Red().Bold()} — Skipping older event {currentAction.Type}:{currentAction.Timestamp % 1000}");
				}
			}

			eventLoopCoroutine = null;
		}

		private IEnumerator SafeLoopCoroutine()
		{
			var wait = new WaitForSeconds(5f);
			while (true)
			{
				yield return wait;
				if (timestampQueue.Count == 0)
					continue;

				instance.eventLoopCoroutine ??= instance.StartCoroutine(instance.EventLoopCoroutine());
			}
		}
	}
}