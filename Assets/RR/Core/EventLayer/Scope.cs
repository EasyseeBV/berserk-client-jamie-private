using System;
using System.Collections.Generic;

//--------------------
//  EXAMPLE OF USAGE
//--------------------
//
// Scope.Event<int>("MyEvent").Subscribe(...);
// Scope.Event<int>("MyEvent").Publish(...);
//

namespace RR.Core.EventLayer
{
	/// <summary>
	/// Lightweight Bus with String as key of event.
	/// </summary>
	public static class Scope
	{
		private static readonly Dictionary<ScopeKey, RREventBase> keyToEvent = new Dictionary<ScopeKey, RREventBase>();

		/// <summary>Get Event by Key</summary>
		public static RREvent Event(string scopeKey)
		{
			var key = new ScopeKey(typeof(RREvent), scopeKey);
			return GetOrCreateEvent<RREvent>(key);
		}

		/// <summary>Get Event by Key</summary>
		public static RREvent<T> Event<T>(string scopeKey)
		{
			var key = new ScopeKey(typeof(RREvent<T>), scopeKey);
			return GetOrCreateEvent<RREvent<T>>(key);
		}

		/// <summary>Get Event by Key</summary>
		public static RREvent<T1, T2> Event<T1, T2>(string scopeKey)
		{
			var key = new ScopeKey(typeof(RREvent<T1, T2>), scopeKey);
			return GetOrCreateEvent<RREvent<T1, T2>>(key);
		}

		/// <summary>Get State by Key</summary>
		public static State<T> State<T>(string scopeKey)
		{
			var key = new ScopeKey(typeof(State<T>), scopeKey);
			return GetOrCreateEvent<State<T>>(key);
		}

        /// <summary>Get State by Key</summary>
        public static State<T, TK> State<T, TK>(string scopeKey)
        {
            var key = new ScopeKey(typeof(State<T, TK>), scopeKey);
            return GetOrCreateEvent<State<T, TK>>(key);
        }

        /// <summary>
        /// Unsubscribe all callbacks of all Events and clear cache
        /// </summary>
        public static void UnsubscribeAndRemoveAll()
		{
			foreach (var e in keyToEvent.Values)
				e.UnsubscribeAll();

			keyToEvent.Clear();
		}

		/// <summary>
		/// Unsubscribe all callbacks and remove Event from cache
		/// </summary>
		public static bool UnsubscribeAndRemoveEvent(string scopeKey)
		{
			var key = new ScopeKey(typeof(RREvent), scopeKey);
			return UnsubscribeAndRemove(key);
		}

		/// <summary>
		/// Unsubscribe all callbacks and remove Event from cache
		/// </summary>
		public static bool UnsubscribeAndRemoveEvent<T>(string scopeKey)
		{
			var key = new ScopeKey(typeof(RREvent<T>), scopeKey);
			return UnsubscribeAndRemove(key);
		}

		/// <summary>
		/// Unsubscribe all callbacks and remove Event from cache
		/// </summary>
		public static bool UnsubscribeAndRemoveEvent<T1, T2>(string scopeKey)
		{
			var key = new ScopeKey(typeof(RREvent<T1, T2>), scopeKey);
			return UnsubscribeAndRemove(key);
		}

		/// <summary>
		/// Unsubscribe all callbacks and remove State from cache
		/// </summary>
		public static bool UnsubscribeAndRemoveState<T>(string scopeKey)
		{
			var key = new ScopeKey(typeof(State<T>), scopeKey);
			return UnsubscribeAndRemove(key);
		}

		#region Private

		private static T GetOrCreateEvent<T>(ScopeKey key) where T : RREventBase, new()
		{
			if (!keyToEvent.TryGetValue(key, out var e))
				keyToEvent[key] = e = new T { Name = key.Key };

			return (T)e;
		}

		private static bool UnsubscribeAndRemove(ScopeKey key)
		{
			if (!keyToEvent.TryGetValue(key, out var e))
				return false;

			e.UnsubscribeAll();
			keyToEvent.Remove(key);
			return true;
		}

		struct ScopeKey
		{
			private Type EventType;
			public string Key;

			public ScopeKey(Type eventType, string key)
			{
				EventType = eventType;
				Key = key ?? throw new Exception("ScopeKey can not be null");
			}

			public override bool Equals(object obj)
			{
				if (!(obj is ScopeKey))
				{
					return false;
				}

				var key = (ScopeKey)obj;
				return EqualityComparer<Type>.Default.Equals(EventType, key.EventType) &&
					   Key == key.Key;
			}

			public override int GetHashCode()
			{
				var hashCode = -614245237;
				hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(EventType);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Key);
				return hashCode;
			}
		}

		#endregion
	}
}