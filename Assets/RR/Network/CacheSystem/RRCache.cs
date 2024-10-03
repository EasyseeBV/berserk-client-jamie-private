using RR.Core.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RR.Core.DebugSystem;
using UnityEngine;

namespace RR.Network.CacheSystem
{
	public static class RRCache
	{
		#region Cache

		// ReSharper disable once InconsistentNaming
		private static readonly Dictionary<string, ICacheEntry> _cache = new Dictionary<string, ICacheEntry>();

		private static Dispatcher _dispatcher;
		private static Dispatcher Dispatcher
		{
			get
			{
				if (_dispatcher != null)
					return _dispatcher;

				var anyDispatcher = UnityEngine.Object.FindObjectOfType<Dispatcher>();

				if (anyDispatcher != null)
					return _dispatcher = anyDispatcher;

				_dispatcher = new GameObject(nameof(Dispatcher)).AddComponent<Dispatcher>();
				RRLogger.Warning($"No {nameof(Dispatcher)} was set to {nameof(RRCache)}! Creating new one.");

				return _dispatcher;
			}
		}

		public static void SetDispatcher(Dispatcher dispatcher)
		{
			_dispatcher = dispatcher;
		}

		#endregion

		#region Get

		/// <summary>
		/// Will return object from cache.
		/// IMPORTANT - If you request derived type, or base type of cached type - new instance of cached entry will be returned.
		/// IMPORTANT - If there is no entry in cache - new entry will be created.
		/// </summary>
		/// <returns>
		/// NULL - object is not in cache.
		/// T - object from cache
		/// </returns>
		public static T Get<T>(string cacheId) where T : class
		{
			return GetOrAddEntry<T>(cacheId, null).GetCachedValue();
		}

		public static void Get<T>(string cacheId, Action onSuccess, Func<Task<T>> fetchPromise = null, bool dispatchOnMainThread = true) where T : class
			=> Get(cacheId, x => onSuccess?.Invoke(), fetchPromise, dispatchOnMainThread);

		public static void Get<T>(string cacheId, Action<T> onSuccess, Func<Task<T>> fetchPromise = null, bool dispatchOnMainThread = true) where T : class
		{
			var entry = GetOrAddEntry(cacheId, fetchPromise);
			var value = entry.GetCachedValue();

			if (value != null)
			{
				onSuccess?.Invoke(value);
				return;
			}

			entry.Fetch(onSuccess, dispatchOnMainThread ? Dispatcher : null);
		}

		/// <summary>
		/// Use this with <see cref="RRCache.AddMany{T}(string, IEnumerable{T})"/> to safely extract cached collections.
		/// Will return Enumerable of object from cache.
		/// IMPORTANT - If there is no entry in cache - new entry will be created.
		/// </summary>
		/// <returns>
		/// Empty collection - object is not in cache.
		/// <br/>IEnumerable{T} - objects from cache
		/// </returns>
		public static IEnumerable<T> GetMany<T>(string cacheId)
			=> Get<IEnumerable<T>>(cacheId) ?? Enumerable.Empty<T>();

		/// <summary>
		/// Will return object from cache.
		/// Will perform fetch on another thread.
		/// Will await fetch if cached object is null.
		/// </summary>
		/// <returns>
		/// T - object from cache
		/// </returns>
		public static async Task<T> GetAsync<T>(string cacheId, Func<Task<T>> fetchPromise = null) where T : class
		{
			var entry = GetOrAddEntry(cacheId, fetchPromise);
			var value = entry.GetCachedValue();

			if (value != null)
				return value;

			return await entry.FetchAsync();
		}

		/// <summary>
		/// Use this with <see cref="RRCache.AddMany{T}(string, IEnumerable{T})"/> to safely extract cached collections.
		/// Will return collection either from cache or its fetchingPromise result.
		/// If cache contains value - it will be returned without fetch.
		/// IMPORTANT - If there is no entry in cache - new entry will be created and fetch result will be stored in it.
		/// </summary>
		/// <returns>
		/// Empty collection - object is not in cache and fetching results in nothing.
		/// IEnumerable{T} - objects from cache
		/// </returns>
		public static async Task<IEnumerable<T>> GetManyAsync<T>(string cacheId, Func<Task<IEnumerable<T>>> fetchPromise = null)
			=> await GetAsync(cacheId, fetchPromise) ?? Enumerable.Empty<T>();

		/// <summary>
		/// Will return Linq.First item from collection cached by collectionId
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collectionId">ID of the cached collection.</param>
		/// <param name="predicate">Predicate to find item in collection.</param>
		/// <returns>
		///	{T} Item from collection
		/// Exception if no item found or collection is empty.
		/// </returns>
		public static T First<T>(string collectionId, Func<T, bool> predicate)
			=> GetMany<T>(collectionId).First(predicate);

		/// <summary>
		/// Will return Linq.FirstOrDefault item from collection cached by collectionId
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collectionId">ID of the cached collection.</param>
		/// <param name="predicate">Predicate to find item in collection.</param>
		/// <returns>
		///	{T} Item from collection
		/// NULL when collection non exist or its empty.
		/// </returns>
		public static T FirstOrDefault<T>(string collectionId, Func<T, bool> predicate)
			=> GetMany<T>(collectionId).FirstOrDefault(predicate);

		public static T Single<T>(string collectionId, Func<T, bool> predicate)
			=> GetMany<T>(collectionId).Single(predicate);

		public static T SingleOrDefault<T>(string collectionId, Func<T, bool> predicate)
			=> GetMany<T>(collectionId).SingleOrDefault(predicate);

		public static IEnumerable<T> Where<T>(string collectionId, Func<T, bool> predicate)
			=> GetMany<T>(collectionId).Where(predicate);

		public static IEnumerable<T> OrderBy<T>(string collectionId, Func<T, bool> predicate)
			=> GetMany<T>(collectionId).OrderBy(predicate);

		public static IEnumerable<T> OrderByDescending<T>(string collectionId, Func<T, bool> predicate)
			=> GetMany<T>(collectionId).OrderByDescending(predicate);

		/// <summary>
		/// Sugar for T Get<T>(string ID) method. Grabs cache entry from cache.
		/// IMPORTANT - If there is no entry in cache - new entry will be created.
		/// </summary>
		public static T Get<T>(string type, string id) where T : class
			=> Get<T>($"{type}/{id}");

		#endregion

		#region Add

		/// <summary>
		/// Append an item to the end of stored collection.
		/// Will use comparer if one provided to determine equality between objects.
		/// Will throw <see cref="InvalidCacheTypeException"/> if appended item type
		/// does not match the cached collection item type, or cached type is not a collection.
		/// </summary>
		/// <returns>
		/// IEnumerable with appended value
		/// Exception when type mismatch
		/// </returns>
		public static ICacheEntry<IEnumerable<T>> AppendUniq<T>(string id, T value, Func<T, bool> comparer = null)
		{
			var entry = GetOrAddEntry<IEnumerable<T>>(id, null);
			var cached = entry.GetCachedValue()?.ToList() ?? new List<T>();

			var item = comparer == null
				? cached.FirstOrDefault(x => x.Equals(value))
				: cached.FirstOrDefault(comparer);

			if (item != null)
			{
				cached[cached.IndexOf(item)] = value;
				entry.TrySetCached(cached);

				return entry;
			}

			if (!entry.TrySetCached(cached.Append(value)))
			{
				throw new InvalidCacheTypeException($"{nameof(RRCache)}.{nameof(Append)} cant Append" +
													$" {nameof(id)}:[{id}] = Type mismatch! [{value.GetType()}] " +
													$"not match entry type:[{entry.GetCachedValue().GetType()}]");
			}

			return entry;
		}

		/// <summary>
		/// Append an item to the end of stored collection.
		/// Will throw <see cref="InvalidCacheTypeException"/> if appended item type
		/// does not match the cached collection item type, or cached type is not a collection.
		/// </summary>
		/// <returns>
		/// IEnumerable with appended value
		/// </returns>
		public static ICacheEntry<IEnumerable<T>> Append<T>(string id, T value)
		{
			var entry = GetOrAddEntry<IEnumerable<T>>(id, null);
			var cached = entry.GetCachedValue() ?? new List<T>();

			if (!entry.TrySetCached(cached.Append(value)))
				throw new InvalidCacheTypeException($"{nameof(RRCache)}.{nameof(Append)} cant Append {nameof(id)}:[{id}] = Type mismatch! [{value.GetType()}] not match entry type:[{entry.GetCachedValue().GetType()}]");

			return entry;
		}

		/// <summary>
		/// Append a collection to the end of stored collection.
		/// Will throw <see cref="InvalidCacheTypeException"/> if appended item type
		/// does not match the cached collection item type, or cached type is not a collection.
		/// </summary>
		/// <returns>
		/// IEnumerable with appended value
		/// </returns>
		public static ICacheEntry<IEnumerable<T>> AppendMany<T>(string id, IEnumerable<T> value)
		{
			var entry = GetOrAddEntry<IEnumerable<T>>(id, null);
			var cached = entry.GetCachedValue() ?? new List<T>();

			if (!entry.TrySetCached(cached.Concat(value)))
				throw new InvalidCacheTypeException($"{nameof(RRCache)}.{nameof(Append)} cant Append {nameof(id)}:[{id}] = Type mismatch! [{value.GetType()}] not match entry type:[{entry.GetCachedValue().GetType()}]");

			return entry;
		}

		/// <summary>
		/// Adds collection of T objects to cache.
		/// Use this to add and later safely extract Enumerable of T items.
		/// IMPORTANT: Will replace existing entity.
		/// </summary>
		public static ICacheEntry<IEnumerable<T>> AddMany<T>(string id, IEnumerable<T> models)
			=> Add(new CacheEntry<IEnumerable<T>>(id, models));

		/// <summary>
		/// Adds collection of T fetching promises to cache.
		/// Use this to add and later safely extract Enumerable of T items.
		/// IMPORTANT: Will replace existing entity.
		/// </summary>
		public static ICacheEntry<IEnumerable<T>> AddMany<T>(string id, Func<Task<IEnumerable<T>>> fetchPromise)
			=> Add(new CacheEntry<IEnumerable<T>>(id, fetchPromise));

		/// <summary>
		/// Adds T object to cache.
		/// IMPORTANT: Will replace existing entity.
		/// </summary>
		public static ICacheEntry<T> Add<T>(string id, T model) where T : class
			=> Add(new CacheEntry<T>(id, model));

		/// <summary>
		/// Adds promise to cache.
		/// Use Add().Fetch() pattern to grab the actual model and store it in cache.
		/// IMPORTANT: Will replace existing entity.
		/// </summary>
		public static ICacheEntry<T> Add<T>(string id, Func<Task<T>> fetchPromise) where T : class
			=> Add(new CacheEntry<T>(id, fetchPromise));

		#endregion

		#region Refresh

		/// <summary>
		/// Refreshes (Fetches) the model in cache using its stored FetchingPromise
		/// </summary>
		public static void Refresh(string cacheId, Action onFetchComplete = null, bool dispatchOnMainThread = true)
		{
			if (!_cache.TryGetValue(cacheId, out var cacheEntry))
			{
				RRLogger.Error($"{nameof(RRCache)}, {nameof(Refresh)} cant find entry in cache with {nameof(cacheId)}:[{cacheId}]");
				return;
			}

			var dispatcher = dispatchOnMainThread
				? Dispatcher
				: null;

			cacheEntry.Fetch(onFetchComplete, dispatcher);
		}

		public static void Refresh<T>(string cacheId, T value)
		{
			if (!_cache.TryGetValue(cacheId, out var cacheEntry))
			{
				RRLogger.Error($"{nameof(RRCache)}, {nameof(Refresh)} cant find entry in cache with {nameof(cacheId)}:[{cacheId}]");
				return;
			}

			if (!cacheEntry.TrySetCached(value))
				RRLogger.Error($"{nameof(RRCache)}, {nameof(Refresh)} cant refresh {nameof(cacheId)}:[{cacheId}] = Type mismatch! [{value.GetType()}] not match entry type:[{cacheEntry.GetCachedValue().GetType()}]");
		}

		/// <summary>
		/// Refreshes (Fetches) the model in cache using its stored FetchingPromise
		/// If entry type not match to {T} - warning will be logged.
		/// If there is no entry stored by cacheId, error will be logged.
		/// </summary>
		public static void Refresh<T>(string cacheId, Action<T> onFetchComplete = null, bool dispatchOnMainThread = true)
		{
			if (!_cache.TryGetValue(cacheId, out var cacheEntry))
			{
				RRLogger.Error($"{nameof(RRCache)}, {nameof(Refresh)} cant find entry in cache with {nameof(cacheId)}:[{cacheId}]");
				return;
			}

			var dispatcher = dispatchOnMainThread
				? Dispatcher
				: null;

			if (cacheEntry is ICacheEntry<T> genericEntry)
			{
				genericEntry.Fetch(onFetchComplete, dispatcher);
				return;
			}

			RRLogger.Error($"{nameof(RRCache)}, {nameof(Refresh)} has started fetching on Type:[{cacheEntry.GetType().Name}] instead of requested Type:[{typeof(T).Name}]");
			cacheEntry.Fetch(() => onFetchComplete?.Invoke(default), dispatcher);
		}

		/// <summary>
		/// Refreshes (Fetches) the model in cache using its stored FetchingPromise
		/// Awaiting this will wait for model to be refreshed.
		/// </summary>
		public static async Task RefreshAsync(string cacheId)
		{
			if (!_cache.TryGetValue(cacheId, out var cacheEntry))
			{
				RRLogger.Error($"{nameof(RRCache)}, {nameof(Refresh)} cant find entry in cache with {nameof(cacheId)}:[{cacheId}]");
				return;
			}

			await cacheEntry.FetchAsync();
		}

		/// <summary>
		/// Sugar to remove type-d objects from Cache.
		/// Its done by calling Clear $"{type}/{id}"
		/// </summary>
		public static void Clear(string type, string id)
			=> Clear($"{type}/{id}");

		/// <summary>
		/// Removes entry from cache by its ID.
		/// </summary>
		public static void Clear(string cacheId)
		{
			if (_cache.TryGetValue(cacheId, out var cacheEntry))
				cacheEntry.Clear();
		}

		/// <summary>
		/// Vanish all elements in app cache.
		/// Use this only for test purposes.
		/// </summary>
		public static void ClearAll()
		{
			foreach (var value in _cache.Values)
			{
				value.Clear();
			}
		}

		#endregion

		#region Private

		private static ICacheEntry<T> Add<T>(ICacheEntry<T> cacheEntry)
		{
			if (_cache.ContainsKey(cacheEntry.Id))
			{
				if (cacheEntry.GetFetchPromise() == null && _cache[cacheEntry.Id] is ICacheEntry<T> genericEntry)
					cacheEntry.UpdatePromise(genericEntry.GetFetchPromise());

				_cache[cacheEntry.Id] = cacheEntry;
			}
			else
				_cache.Add(cacheEntry.Id, cacheEntry);

			return cacheEntry;
		}

		/// <summary>
		/// Simply gets cached entity, or adds new one if non exist.
		/// Will throw:
		/// - When user requests entry of type T, but cache has entry of different type.
		/// - When stored cache entity is not of Generic type - that's not allowed by design and can only be introduced by cache modifications.
		/// </summary>
		private static ICacheEntry<T> GetOrAddEntry<T>(string id, Func<Task<T>> fetchPromise) where T : class
		{
			id = id.TrimStart('/');

			if (string.IsNullOrEmpty(id))
				throw new ArgumentNullException($"{nameof(id)} at [{nameof(CacheEntry<T>)}.{nameof(GetOrAddEntry)}] cannot be null or empty string!");

			lock (_cache)
			{
				if (!_cache.TryGetValue(id, out var cacheEntry))
					return Add(new CacheEntry<T>(id, fetchPromise));

				if (cacheEntry is ICacheEntry<T> genericEntry)
					return genericEntry.UpdatePromise(fetchPromise);

				if (TryExtractFromInheritance(cacheEntry, out genericEntry))
					return genericEntry.UpdatePromise(fetchPromise);

				var entryType = cacheEntry.GetType();
				if (!entryType.IsGenericType)
					throw new InvalidCacheTypeException($"[ERROR][CRITICAL] {nameof(cacheEntry)} type: [{entryType}] is not a Generic type! Cant extract {typeof(T)} from it.");

				var entryCachedTypes = string.Join(",", cacheEntry.GetType().GenericTypeArguments.Select(x => x.Name));
				throw new InvalidCacheTypeException($"[ERROR] Cache entry type(-s): [{entryCachedTypes}] not matches user requested type: [{typeof(T)}]. Cant extract proper entry from cache.");
			}
		}

		/// <summary>
		/// SLOW! Please avoid this. Only for some extra cases.
		/// Used to extract cached entry from inherited types and interfaces.
		///
		/// Example:
		///
		/// Cache contains TestItem, that inherits from ITestItem and TestItemBase:
		/// Cache.Add(new TestItem());
		///
		/// User can access this item by using any of its types:
		///
		/// TestItem = Cache.Get{TestItem}(type)  // ok!
		/// TestItemBase = Cache.Get{TestItemBase}(type)  // ok!
		/// ITestItem = Cache.Get{ITestItem}(type) // ok!
		///
		/// No magic.
		///
		/// !!!! One thing is IMPORTANT to know !!!!
		/// When type conversion occurs, NEW instance of <see cref="CacheEntry{T}"/> is returned.
		/// Be careful - easy to use incorrect for a scenarios, where entry is added and later fetched from same reference.
		/// </summary>
		private static bool TryExtractFromInheritance<T>(ICacheEntry cacheEntry, out ICacheEntry<T> genericEntry) where T : class
		{
			var cachedType = cacheEntry.GetType().GenericTypeArguments.Single();
			var typeOfT = typeof(T);

			if (typeOfT.IsAssignableFrom(cachedType) || cachedType.IsAssignableFrom(typeOfT))
			{
				var genericType = typeof(CacheEntry<>).MakeGenericType(typeOfT);
				var newEntry = Activator.CreateInstance(genericType, cacheEntry);

				genericEntry = (ICacheEntry<T>)newEntry;

				RRLogger.Warning($"{nameof(RRCache)}, {nameof(TryExtractFromInheritance)} happened. New entry instance of Type:[{genericType}] is returned.");

				return true;
			}

			genericEntry = null;
			return false;
		}

		#endregion
	}
}
