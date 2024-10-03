using System;
using System.Collections;
using UnityEngine;

namespace RR.UI.Custom
{
	public class RRBehavior : MonoBehaviour
	{
		public void SetGameObjectActive(bool value = true)
		{
			gameObject.SetActive(value);
		}

		public void DoNextFrame(Action action, int frameCount = 1)
			=> DoAfterFrame(action, frameCount);

		public void DoAfterFrame(Action action, int frameCount = 1)
			=> StartCoroutine(DoAfterFrameRoutine(action, frameCount));

		private IEnumerator DoAfterFrameRoutine(Action action, int frameCount = 1)
		{
			for (var i = 0; i < frameCount; i++)
				yield return null;

			action.Invoke();
		}

#if UNITY_EDITOR

		private void Reset()
		{
			var lowerName = name.ToLower();
			if (lowerName.EndsWith("manager")
				|| lowerName.EndsWith("resolver")
				|| lowerName.EndsWith("handler"))
			{
				name = GetType().Name;
			}
		}
#endif
	}
}