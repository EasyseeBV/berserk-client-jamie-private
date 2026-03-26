using System;
using UnityEngine;

namespace UI
{
	public static class BoardLayoutSettings
	{
		public const string PlayerPrefsKey = "BoardLayout";
		public const string ClassicValue = "classic";
		// Keep the stored value as "compact" for backwards compatibility.
		public const string MinimalValue = "compact";

		private static bool? runtimeCompactOverride;

		public static event Action Changed;

		public static string GetSelectedValue()
		{
			var value = PlayerPrefs.GetString(PlayerPrefsKey, ClassicValue);
			return value == MinimalValue
				? MinimalValue
				: ClassicValue;
		}

		public static bool IsMinimal()
		{
			return GauntletMatchPresentation.ForceCompact
				|| (runtimeCompactOverride ?? (GetSelectedValue() == MinimalValue));
		}

		public static bool IsCompact()
		{
			return IsMinimal();
		}

		public static void SetRuntimeOverride(bool value)
		{
			if (runtimeCompactOverride == value)
				return;

			runtimeCompactOverride = value;
			Changed?.Invoke();
		}

		public static void ClearRuntimeOverride()
		{
			if (!runtimeCompactOverride.HasValue)
				return;

			runtimeCompactOverride = null;
			Changed?.Invoke();
		}

		public static void SetCompact(bool value)
		{
			var next = value
				? MinimalValue
				: ClassicValue;

			if (GetSelectedValue() == next)
				return;

			PlayerPrefs.SetString(PlayerPrefsKey, next);
			PlayerPrefs.Save();
			Changed?.Invoke();
		}
	}
}
