using System;
using UnityEngine.Events;

namespace RR.Core.Components
{
    [Serializable]
    public class GenericUnityEvent<T> : UnityEvent<T>
    {
    }

    [Serializable]
    public class GenericUnityEvent : GenericUnityEvent<object>
    {
    }
}
