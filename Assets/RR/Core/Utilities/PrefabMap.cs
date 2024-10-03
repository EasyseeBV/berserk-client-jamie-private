using System.Linq;
using RR.Core.Components;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.Core.Serialization;
using UnityEngine;

namespace RR.Core.Utilities
{
    [DefaultExecutionOrder(-15)]
    public class PrefabMap : Singleton<PrefabMap>
    {
        public static UnitySerializedDictionary<string, Component> Map
        {
            get
            {
                if (Instance == null)
                    Instance = FindObjectOfType<PrefabMap>();
                return Instance.map;
            }
        }

        [SerializeField]
        protected UnitySerializedDictionary<string, Component> map
            = new UnitySerializedDictionary<string, Component>();

        public UnitySerializedDictionary<string, Component> GetMap() => map;

        public static Component CreateInstance(string prefabId,
            Vector3 position,
            Transform parent = null,
            Quaternion rotation = default,
            bool ignoreCase = false)
        {
            var prefab = GetPrefab(prefabId, ignoreCase);
            if (prefab == null)
                return null;

            var quaternion = rotation == default ? Quaternion.identity : rotation;
            var instance = parent != null
                ? Instantiate(prefab, position, quaternion, parent)
                : Instantiate(prefab, position, quaternion);

            return instance;
        }

        public static T CreateInstance<T>(string prefabId,
            Vector3 position,
            Transform parent = null,
            Quaternion rotation = default,
            bool ignoreCase = false) where T : Component
        {
            var prefab = GetPrefab<T>(prefabId, ignoreCase);
            if (prefab == null)
                return null;

            var quaternion = rotation == default ? Quaternion.identity : rotation;
            var instance = parent != null
                ? Instantiate(prefab, position, quaternion, parent)
                : Instantiate(prefab, position, quaternion);

            return instance;
        }

        public static Component GetPrefab(string prefabId, bool ignoreCase = false)
        {
            if (ignoreCase)
            {
                var prefab = Map.FirstOrDefault(x => x.Key == prefabId).Value;
                if (prefab != default)
                    return prefab;
            }
            else if (Map.TryGetValue(prefabId, out var prefab))
                return prefab;

            RRLogger.Error($"There is no prefab {prefabId} in the map.");
            return null;
        }

        public static T GetPrefab<T>(string prefabId, bool ignoreCase = false) where T : Component
        {
            if (ignoreCase)
            {
                var prefab = Map.FirstOrDefault(x => x.Key == prefabId).Value;
                if (prefab != default)
                    return prefab is T result ? result : prefab.GetComponent<T>();
            }
            else if (Map.TryGetValue(prefabId, out var prefab))
                return prefab is T result ? result : prefab.GetComponent<T>();

            RRLogger.Error($"There is no prefab {prefabId} in the map.");
            return null;
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            if (!map.ContainsValue(null))
                return;

            map.Where(x => x.Value == null || !x.Value).ToList().ForEach(x =>
            {
                RRLogger.Log($"Removed null entry in map: {x.Key.Red()}");
                map.Remove(x.Key);
            });
        }

#endif

    }
}
