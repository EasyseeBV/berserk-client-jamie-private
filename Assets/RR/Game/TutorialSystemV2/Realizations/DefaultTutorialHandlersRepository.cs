using System;
using System.Collections.Generic;
using System.Linq;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;

namespace RR.Game.TutorialSystemV2.Realizations
{
	public class DefaultTutorialHandlersRepository : ITutorialHandlersRepository, IDisposable
	{
		protected readonly IDictionary<string, IList<ITutorialHandler>> Handlers;

		public DefaultTutorialHandlersRepository()
		{
			Handlers = new Dictionary<string, IList<ITutorialHandler>>();
		}

		public void Register(ITutorialHandler value)
		{
			var ids = GetHandlerIds(value);
			if (ids.Length == 0)
				return;

			foreach (var id in ids)
			{
				if (string.IsNullOrEmpty(id))
					continue;
				
				if (!Handlers.TryGetValue(id, out var handlers))
					Handlers[id] = handlers = new List<ITutorialHandler>();

				if (handlers.Contains(value))
				{
					RRLogger.Log($"[{GetType().Name.Orange()}] Can't add the same handler {value} twice with id {id}");
					continue;
				}

				handlers.Add(value);
			}

			RRLogger.Log($"[{GetType().Name.Orange()}] Handler : {value} added.");
		}

		public void UnRegister(ITutorialHandler value)
		{
			var ids = GetHandlerIds(value);
			if (ids.Length == 0)
			{
				RRLogger.Error($"[{GetType().Name.Orange()}] Can't remove handler, ids or instance is missing.");
				return;
			}

			foreach (var id in ids)
			{
				if (string.IsNullOrEmpty(id))
					continue;
				
				if (!Handlers.TryGetValue(id, out var handlers))
					return;

				handlers.Remove(value);
				if (handlers.Count == 0)
					Handlers.Remove(id);

				if (value is not ITutorialRemoveHandler handler)
					continue;
				
				try
				{
					handler.RemovedAsync().Forget();
				}
				catch (Exception e)
				{
					RRLogger.Error($"[{GetType().Name.Orange()}] While unregister thr handler : {handler}, throw an exception : {e}");
				}
			}

			RRLogger.Log($"[{GetType().Name.Orange()}] Handler {value}, removed.");
		}

		public void ResetAll()
		{
			GetAll().ForEach(UnRegister);
			Handlers.Clear();
		}

		public void Dispose()
		{
			ResetAll();
		}

		public IEnumerable<T> GetAll<T>() where T : ITutorialHandler
		{
			return GetAll().OfType<T>().ToArray();
		}

		public IEnumerable<ITutorialHandler> GetAll()
		{
			return Handlers
				.SelectMany(x => x.Value)
				.OrderIfPossible()
				.ToArray();
		}

		public IEnumerable<T> Get<T>(bool includeNonIdentity, params string[] ids) where T : ITutorialHandler
		{
			return Get(includeNonIdentity, ids).OfType<T>().ToArray();
		}

		public IEnumerable<ITutorialHandler> Get(bool includeNonIdentity, params string[] ids)
		{
			if (ids == null || ids.Length == 0)
				return Array.Empty<ITutorialHandler>();

			return Handlers
				.Where(kpv => ids.Contains(kpv.Key) || (includeNonIdentity && kpv.Value.Any(IsNonIdentity)))
				.SelectMany(x => x.Value)
				.OrderIfPossible()
				.ToArray();
		}

		public IEnumerable<T> GetNonIdentity<T>() where T : ITutorialHandler
		{
			return GetNonIdentity().OfType<T>().ToArray();
		}

		public IEnumerable<ITutorialHandler> GetNonIdentity()
		{
			return Handlers
				.SelectMany(x => x.Value.Where(IsNonIdentity))
				.OrderIfPossible()
				.ToArray();
		}

		protected virtual string[] GetHandlerIds(ITutorialHandler value)
		{
			if (value is ITutorialIdentity {Ids: {Length: > 0}} identity)
				return identity.Ids.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToArray();

			return value == null ? Array.Empty<string>() : new[] {value.GetType().Name};
		}

		protected virtual bool IsNonIdentity(ITutorialHandler value)
		{
			return value is not ITutorialIdentity;
		}
	}
}