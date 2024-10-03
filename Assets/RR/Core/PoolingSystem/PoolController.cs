using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = RR.Core.DebugSystem.RRLogger;

namespace RR.Core.PoolingSystem
{
    /// <summary>
    /// GameObject/Component Pool.
    /// Allows to create and reuse GameObject/Components from prefab.
    /// Drag this controller on any GameObject.
    /// </summary>
    public class PoolController : MonoBehaviour
    {
        public int DefaultCount = 20;

        private Dictionary<GameObject, Pool> prefabToPool = new Dictionary<GameObject, Pool>();
        private Dictionary<GameObject, Pool> objectToPool = new Dictionary<GameObject, Pool>();
        private static PoolController instance;

        public static PoolController Instance { get => GetOrCreateInstance(); private set => instance = value; }

        class Pool
        {
            public LinkedList<GameObject> InUse = new LinkedList<GameObject>();
            public LinkedList<GameObject> NoUse = new LinkedList<GameObject>();
        }

        void Awake()
        {
            if (instance == null)
                Instance = this;
        }

        private static PoolController GetOrCreateInstance()
        {
            if (instance == null)
            {
                Debug.Warning($"{nameof(PoolController)} is not found on Scene. It will be created automatically.");
                var go = new GameObject() { name = nameof(PoolController) };
                instance = go.AddComponent<PoolController>();
            }

            return instance;
        }

        /// <summary>Creates pool with given count of objects</summary>
        public void PreparePool(GameObject prefab, int count)
        {
            GetOrCreatePool(prefab, count);
        }

        /// <summary>Creates pool with given count of objects</summary>
        public void PreparePool(Component prefab, int count)
        {
            GetOrCreatePool(prefab.gameObject, count);
        }

        /// <summary>Return component to pool</summary>
        public void ReturnToPool(Component component)
        {
            ReturnToPool(component.gameObject);
        }

        /// <summary>Return GameObject to pool</summary>
        public void ReturnToPool(GameObject obj)
        {
            if (obj == null)
                return;

            if (!objectToPool.TryGetValue(obj, out var pool))
            {
                //Destroy(obj);
                return;
            }

            obj.transform.SetParent(this.transform, false);
            obj.SetActive(false);
            if (pool.InUse.Remove(obj))
            {
                pool.NoUse.AddLast(obj);
            }
        }

        /// <summary>Return all children objects of parent</summary>
        public void ReturnAllChildren(GameObject parent)
        {
            var tr = parent.transform;
            for (int i = tr.childCount - 1; i >= 0; i--)
            {
                ReturnToPool(tr.GetChild(i).gameObject);
            }
        }

        /// <summary>Get GameObject from pool</summary>
        public GameObject CreateFromPool(GameObject prefab, Transform parent)
        {
            var obj = CreateFromPoolInternal(prefab.gameObject, parent);
            obj.transform.SetParent(parent, false);
            return obj;
        }

        /// <summary>Get component from pool</summary>
        public T CreateFromPool<T>(T prefab, Transform parent) where T : Component
        {
            var obj = CreateFromPoolInternal(prefab.gameObject, parent);
            obj.transform.SetParent(parent, false);
            return obj.GetComponent<T>();
        }

        /// <summary>Get GameObject from pool</summary>
        public GameObject CreateFromPool(GameObject prefab, Vector3 pos, Quaternion rotation, Transform parent = null)
        {
            var obj = CreateFromPoolInternal(prefab.gameObject, parent);
            if (parent != null)
                obj.transform.SetParent(parent, false);
            obj.transform.position = pos;
            obj.transform.rotation = rotation;
            return obj;
        }

        /// <summary>Get component from pool</summary>
        public T CreateFromPool<T>(T prefab, Vector3 pos, Quaternion rotation, Transform parent = null) where T : Component
        {
            var obj = CreateFromPoolInternal(prefab.gameObject, parent);
            if (parent != null)
                obj.transform.SetParent(parent, false);
            obj.transform.position = pos;
            obj.transform.rotation = rotation;
            return obj.GetComponent<T>();
        }

        /// <summary>Get GameObject from pool</summary>
        public GameObject CreateFromPool(GameObject prefab)
        {
            return CreateFromPoolInternal(prefab, null);
        }

        private GameObject CreateFromPoolInternal(GameObject prefab, Transform holder)
        {
            if (!prefab)
                return null;

            var pool = GetOrCreatePool(prefab, DefaultCount);

            GameObject item = null;
            while (pool.NoUse.Count > 0)
            {
                item = pool.NoUse.Last.Value;
                pool.NoUse.RemoveLast();
                if (!item)//destroyed?
                {
                    continue;
                }
                pool.InUse.AddLast(item);
                item.SetActive(true);

                return item;
            }

            //no items in pool => create new
            item = Instantiate(prefab, holder ?? transform);
            pool.InUse.AddLast(item);
            objectToPool[item] = pool;

            return item;
        }

        private void RemoveDestroyed(Pool pool)
        {
            var list = pool.InUse;
            if (list.Count == 0) return;

            var first = list.First;
            list.RemoveFirst();

            //is destroyed?
            if (!first.Value)
            {
                //remove from pool
                objectToPool.Remove(first.Value);
            }
            else
            {
                //stil in pool
                list.AddLast(first);
            }
        }

        private Pool GetOrCreatePool(GameObject prefab, int count)
        {
            if (prefabToPool.TryGetValue(prefab, out var pool))
                return pool;

            prefabToPool[prefab] = pool = new Pool();

            for (int i = 0; i < count; i++)
            {
                var obj = Instantiate(prefab, transform);
                obj.SetActive(false);
                pool.NoUse.AddLast(obj);
                objectToPool[obj] = pool;
            }

            return pool;
        }
    }

    /// <summary>
    /// Provides static Pool of GameObjects/Components.
    /// </summary>
    public static class Pool
    {
        #region Public static methods

        /// <summary>Get GameObject from pool</summary>
        public static GameObject Create(GameObject prefab, Vector3 pos, Transform parent = null)
        {
            return PoolController.Instance.CreateFromPool(prefab, pos, Quaternion.identity, parent);
        }

        /// <summary>Get GameObject from pool</summary>
        public static GameObject Create(GameObject prefab, Vector3 pos, Quaternion rotation, Transform parent = null)
        {
            return PoolController.Instance.CreateFromPool(prefab, pos, rotation, parent);
        }

        /// <summary>Get GameObject from pool</summary>
        public static GameObject Create(GameObject prefab, Transform parent = null)
        {
            return PoolController.Instance.CreateFromPool(prefab, parent);
        }

        /// <summary>Get component from pool</summary>
        public static T Create<T>(T prefab, Transform parent = null) where T : Component
        {
            return PoolController.Instance.CreateFromPool<T>(prefab, parent);
        }

        /// <summary>Get component from pool</summary>
        public static T Create<T>(T prefab, Vector3 pos, Transform parent = null) where T : Component
        {
            return PoolController.Instance.CreateFromPool<T>(prefab, pos, Quaternion.identity, parent);
        }

        /// <summary>Get component from pool</summary>
        public static T Create<T>(T prefab, Vector3 pos, Quaternion rotation, Transform parent = null) where T : Component
        {
            return PoolController.Instance.CreateFromPool<T>(prefab, pos, rotation, parent);
        }

        /// <summary>Return GameObject to pool</summary>		
        public static void Return(GameObject createdObject)
        {
            PoolController.Instance.ReturnToPool(createdObject);
        }

        /// <summary>Return component to pool</summary>
        public static void Return(Component component)
        {
            PoolController.Instance.ReturnToPool(component);
        }

        /// <summary>Return all children objects of parent</summary>
        public static void ReturnAllChildren(GameObject parent)
        {
            PoolController.Instance.ReturnAllChildren(parent);
        }

        /// <summary>Create pool with given size</summary>		
        public static void PreparePool(GameObject prefab, int count)
        {
            PoolController.Instance.PreparePool(prefab, count);
        }

        /// <summary>Create pool with given size</summary>
        public static void PreparePool(Component prefab, int count)
        {
            PoolController.Instance.PreparePool(prefab, count);
        }

        #endregion
    }
}