using System;
using UnityEngine.UI;

namespace RR.Core.Extensions
{
	public static class UnityUIExtensions
	{
		public static void Subscribe(this Button button, Action action)
		{
			button.onClick.AddListener(action.Invoke);
		}

		public static void Subscribe<T>(this Button button, Func<T> action)
		{
			button.onClick.AddListener(() => action.Invoke());
		}

		/// <summary>
		/// Add listener and send DEFAULT as a parameter;
		/// </summary>
		public static void Subscribe<T>(this Button button, Action<T> action)
		{
			button.onClick.AddListener(() => action.Invoke(default));
		}
	}
}