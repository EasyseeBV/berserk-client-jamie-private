using Sirenix.OdinInspector;
using UnityEngine;

namespace RR.InternalTools
{
	public class SerializedSingleton<T> : SerializedMonoBehaviour where T : SerializedMonoBehaviour
    {
        protected static T Instance;
        protected void Awake()
        {
	        if (Instance == null)
	        {
		        Instance = this as T;
	        }
	        else if (Instance != this)
	        {
		        Destroy(gameObject);
		        return;
	        }

	        if (Application.isPlaying)
		        DontDestroyOnLoad(this);

	        OnAwake();
        }

        protected virtual void OnAwake() { }

#if UNITY_EDITOR
        private void Reset()
        {
	        name = $"{GetType().Name}_Slton";
        }
#endif
	}
}