using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI
{
    public abstract partial class AutoMap<T> : SerializedScriptableObject where T : Object
    {
        [Header("Map settings")]
        [SerializeField] protected Dictionary<string, T> map = default;
        [SerializeField] protected string assetsPath = default;
    }
}