using System;
using UnityEngine;

namespace UI
{
	public readonly struct ArenaThemeDefinition
	{
		public ArenaThemeDefinition(
			string preferenceValue,
			string displayName,
			string backgroundResourceId,
			string gameboardResourceId,
			string topLeftCornerResourceId,
			string topRightCornerResourceId,
			string bottomLeftCornerResourceId,
			string bottomRightCornerResourceId)
		{
			PreferenceValue = preferenceValue;
			DisplayName = displayName;
			BackgroundResourceId = backgroundResourceId;
			GameboardResourceId = gameboardResourceId;
			TopLeftCornerResourceId = topLeftCornerResourceId;
			TopRightCornerResourceId = topRightCornerResourceId;
			BottomLeftCornerResourceId = bottomLeftCornerResourceId;
			BottomRightCornerResourceId = bottomRightCornerResourceId;
		}

		public string PreferenceValue { get; }
		public string DisplayName { get; }
		public string BackgroundResourceId { get; }
		public string GameboardResourceId { get; }
		public string TopLeftCornerResourceId { get; }
		public string TopRightCornerResourceId { get; }
		public string BottomLeftCornerResourceId { get; }
		public string BottomRightCornerResourceId { get; }
	}

	public static class ArenaThemeSettings
	{
		public const string PreferenceKey = "SelectedArenaTheme";
		public const string Classic = "classic";
		public const string Boreas = "boreas";
		public const string Arcadia = "arcadia";
		public const string Hades = "hades";
		public const string Notus = "notus";
		public const string Colosseum = "colosseum";

		private const string TopLeftDefault = "Corner_Top_Left_Boreas_Default";
		private const string TopRightDefault = "Corner_Top_Right_Arcadia_Default";
		private const string BottomLeftDefault = "Corner_Bottom_Left_Hades_Default";
		private const string BottomRightDefault = "Corner_Bottom_Right_Notus_Default";

		private static readonly ArenaThemeDefinition[] Definitions =
		{
			new(
				Classic,
				"Classic",
				"Background_Fire",
				"Gameboard_Neutral",
				TopLeftDefault,
				TopRightDefault,
				BottomLeftDefault,
				BottomRightDefault),
			new(
				Boreas,
				"Boreas",
				"Background_Boreas_V2",
				"Gameboard_Boreas",
				TopLeftDefault,
				TopRightDefault,
				BottomLeftDefault,
				BottomRightDefault),
			new(
				Arcadia,
				"Arcadia",
				"Background_Arcadia_V2",
				"Gameboard_Arcadia",
				TopLeftDefault,
				TopRightDefault,
				BottomLeftDefault,
				BottomRightDefault),
			new(
				Hades,
				"Hades",
				"Background_Hades_V2",
				"Gameboard_Hades",
				TopLeftDefault,
				TopRightDefault,
				BottomLeftDefault,
				BottomRightDefault),
			new(
				Notus,
				"Notus",
				"Background_Notus_V2",
				"Gameboard_Notus",
				TopLeftDefault,
				TopRightDefault,
				BottomLeftDefault,
				BottomRightDefault)
			,
			new(
				Colosseum,
				"Colosseum",
				"Background_Colosseum",
				"Gameboard_Colosseum",
				TopLeftDefault,
				TopRightDefault,
				BottomLeftDefault,
				BottomRightDefault)
		};

		private static string runtimeOverrideValue;

		public static event Action Changed;

		public static ArenaThemeDefinition Current => GetDefinition(GetCurrentValue());

		public static ArenaThemeDefinition[] GetDefinitions()
		{
			return Definitions;
		}

		public static string GetSelectedValue()
		{
			var rawValue = PlayerPrefs.GetString(PreferenceKey, Classic);
			return GetDefinition(rawValue).PreferenceValue;
		}

		public static void SetRuntimeOverride(string preferenceValue)
		{
			var resolved = GetDefinition(preferenceValue).PreferenceValue;
			if (string.Equals(runtimeOverrideValue, resolved, StringComparison.OrdinalIgnoreCase))
				return;

			runtimeOverrideValue = resolved;
			Changed?.Invoke();
		}

		public static void ClearRuntimeOverride()
		{
			if (string.IsNullOrWhiteSpace(runtimeOverrideValue))
				return;

			runtimeOverrideValue = null;
			Changed?.Invoke();
		}

		public static void SetSelected(string preferenceValue)
		{
			var resolved = GetDefinition(preferenceValue).PreferenceValue;
			PlayerPrefs.SetString(PreferenceKey, resolved);
			PlayerPrefs.Save();
			Changed?.Invoke();
		}

		public static ArenaThemeDefinition GetDefinition(string preferenceValue)
		{
			if (string.Equals(preferenceValue, "vulcan", StringComparison.OrdinalIgnoreCase))
				preferenceValue = Classic;

			foreach (var definition in Definitions)
			{
				if (string.Equals(definition.PreferenceValue, preferenceValue, StringComparison.OrdinalIgnoreCase))
					return definition;
			}

			return Definitions[0];
		}

			private static string GetCurrentValue()
			{
				if (!string.IsNullOrWhiteSpace(GauntletMatchPresentation.ForcedArenaTheme))
					return GetDefinition(GauntletMatchPresentation.ForcedArenaTheme).PreferenceValue;

				return string.IsNullOrWhiteSpace(runtimeOverrideValue)
					? GetSelectedValue()
					: runtimeOverrideValue;
			}
	}
}
