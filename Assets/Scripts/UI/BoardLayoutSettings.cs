using System;
using UnityEngine;

namespace UI
{
	public static class BoardLayoutSettings
	{
		private const string PreferenceKey = "SelectedBoardLayout";
		public const string ClassicValue = "classic";
		// Keep the stored value as "compact" for backwards compatibility.
		public const string MinimalValue = "compact";

		private static bool? runtimeCompactOverride;

		public static event Action Changed;

		public static string GetSelectedValue()
		{
			var selected = PlayerPrefs.GetString(PreferenceKey, ClassicValue);
			if (selected == ClassicValue || selected == MinimalValue)
				return selected;

			PlayerPrefs.SetString(PreferenceKey, ClassicValue);
			PlayerPrefs.Save();
			return ClassicValue;
		}

		public static bool IsMinimal()
		{
			return runtimeCompactOverride ?? (GetSelectedValue() == MinimalValue);
		}

		public static void SetCompact(bool value)
		{
			PlayerPrefs.SetString(PreferenceKey, value ? MinimalValue : ClassicValue);
			PlayerPrefs.Save();
			Changed?.Invoke();
		}

		public static void SetRuntimeOverride(bool value)
		{
			runtimeCompactOverride = value;
			Changed?.Invoke();
		}

		public static void ClearRuntimeOverride()
		{
			runtimeCompactOverride = null;
			Changed?.Invoke();
		}
	}
}
