using System;
using System.Threading.Tasks;
using RR.Core.Async;
using RR.Core.Extensions;

namespace RR.Network.CacheSystem
{
	public interface ICacheEntry
	{
		string Id { get; }

		object GetCachedValue();
		bool TrySetCached(object value);

		void Clear();

		Task FetchAsync();
		ICacheEntry Fetch(Action onFetchComplete = null, Dispatcher dispatcher = null);
	}

	public interface ICacheEntry<T> : ICacheEntry
	{
		new T GetCachedValue();
		new Task<T> FetchAsync();
		Func<Task<T>> GetFetchPromise();

		ICacheEntry<T> Fetch(Action<T> onFetchComplete = null, Dispatcher dispatcher = null);
		ICacheEntry<T> UpdatePromise(Func<Task<T>> fetchPromise);
	}

	internal class CacheEntry<T> : ICacheEntry<T> where T : class
	{
		public string Id { get; }

		private Task<T> fetchingTask;
		private Func<Task<T>> fetchPromise;
		private T cached;

		public CacheEntry(string id)
		{
			Id = id;
		}

		public CacheEntry(string id, T model = null) : this(id)
		{
			if (model != null)
				cached = model;
		}

		public CacheEntry(string id, Func<Task<T>> fetchPromise = default) : this(id)
		{
			if (fetchPromise != null)
				this.fetchPromise = fetchPromise;
		}

		/// <summary>
		/// Used by <see cref="RRCache.TryExtractFromInheritance{T}(ICacheEntry, out ICacheEntry{T})"/>
		/// </summary>
		public CacheEntry(ICacheEntry cacheEntry) : this(cacheEntry.Id)
		{
			var value = cacheEntry.GetCachedValue();

			switch (value)
			{
				case null:
					return;
				case T assignableValue:
					cached = assignableValue;
					break;
				default:
					cached = Activator.CreateInstance<T>();
					value.CopyPropertiesTo(cached).CopyFieldsTo(cached);
					break;
			}
		}

		T ICacheEntry<T>.GetCachedValue() => cached;

		object ICacheEntry.GetCachedValue() => cached;
		public Func<Task<T>> GetFetchPromise() => fetchPromise;

		public void Clear()
		{
			cached = default;
		}

		public bool TrySetCached(object value)
		{
			if (!(value is T allowedValue))
				return false;

			cached = allowedValue;
			return true;
		}

		public ICacheEntry<T> UpdatePromise(Func<Task<T>> fetchPromise)
		{
			if (fetchPromise != null)
				this.fetchPromise = fetchPromise;

			return this;
		}

		Task ICacheEntry.FetchAsync() => FetchAsync();

		public async Task<T> FetchAsync()
		{
			StartOnFetching(null, null);
			return await fetchingTask;
		}

		public ICacheEntry Fetch(Action onFetchComplete = null, Dispatcher dispatcher = null)
			=> StartOnFetching(x => onFetchComplete?.Invoke(), dispatcher);

		public ICacheEntry<T> Fetch(Action<T> onFetchComplete = null, Dispatcher dispatcher = null)
			=> StartOnFetching(onFetchComplete, dispatcher);

		private ICacheEntry<T> StartOnFetching(Action<T> onFetchComplete, Dispatcher dispatcher)
		{
			if (HasFetchingStarted())
			{
				fetchingTask.ContinueWith(x =>
				{
					var result = x.Result;

					if (dispatcher != null)
						dispatcher.Queue(() => onFetchComplete?.Invoke(result));
					else
						onFetchComplete?.Invoke(result);

					return result;
				});

				return this;
			}

			async Task<T> FetchWrapper()
			{
				var result = await PerformFetching();

				if (dispatcher != null)
				{
					dispatcher.Queue(() => onFetchComplete?.Invoke(result));
					return result;
				}

				onFetchComplete?.Invoke(result);
				return result;
			}

			// this assignation execute async operation, starting to fetch
			fetchingTask = FetchWrapper();
			return this;
		}

		private async Task<T> PerformFetching()
		{
			if (fetchPromise == null)
				return cached;

			var tcs = new TaskCompletionSource<T>();

			try
			{
				var result = await fetchPromise();
				cached = result;
				tcs.SetResult(result);
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError($"[ERROR] {Id} failed to {nameof(Fetch)}");
				UnityEngine.Debug.LogError(ex);

				tcs.SetException(ex);
			}

			return await tcs.Task;
		}

		private bool HasFetchingStarted()
		{
			if (fetchingTask == null)
				return false;

			lock (fetchingTask)
			{
				return fetchingTask.Status == TaskStatus.Running
				       || fetchingTask.Status == TaskStatus.Created
				       || fetchingTask.Status == TaskStatus.WaitingForActivation
				       || fetchingTask.Status == TaskStatus.WaitingToRun
				       || fetchingTask.Status == TaskStatus.WaitingForChildrenToComplete;
			}
		}
	}
}
