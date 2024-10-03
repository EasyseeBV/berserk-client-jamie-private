using UnityEngine;

namespace RR.Core.Components
{
	public interface ISingleton { }
	public class Singleton<T> : MonoBehaviour, ISingleton where T : MonoBehaviour
	{
		/// <summary>
		/// We use protected instance to disallow users writing MySingleton.Instance.DoSomething construction.
		/// Please use public static methods to expose your singleton.
		/// </summary>
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
