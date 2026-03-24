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
		public const string Vulcan = "vulcan";
		public const string Boreas = "boreas";
		public const string Arcadia = "arcadia";
		public const string Hades = "hades";
		public const string Notus = "notus";
		public const string Colosseum = "colosseum";
		private const string DefaultTheme = Colosseum;

		private const string TopLeftDefault = "Corner_Top_Left_Boreas_Default";
		private const string TopRightDefault = "Corner_Top_Right_Arcadia_Default";
		private const string BottomLeftDefault = "Corner_Bottom_Left_Hades_Default";
		private const string BottomRightDefault = "Corner_Bottom_Right_Notus_Default";
		private const string TopLeftVulcan = "corner_top_left_vulcan";
		private const string TopRightVulcan = "corner_top_right_vulcan";
		private const string BottomLeftVulcan = "corner_bottom_left_vulcan";
		private const string BottomRightVulcan = "corner_bottom_right_vulcan";

		private static readonly ArenaThemeDefinition[] Definitions =
		{
			new(
				Vulcan,
				"Vulcan",
				"arena_background_vulcan",
				"Gameboard_Vulcan",
				TopLeftVulcan,
				TopRightVulcan,
				BottomLeftVulcan,
				BottomRightVulcan),
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
				BottomRightDefault),
			new(
				Colosseum,
				"Colosseum",
				"Background_Colosseum",
				"Gameboard_Colosseum",
				TopLeftVulcan,
				TopRightVulcan,
				BottomLeftVulcan,
				BottomRightVulcan)
		};

#if UNITY_EDITOR
		private static bool editorDefaultApplied;
#endif

		public static event Action Changed;

		public static ArenaThemeDefinition Current => GetDefinition(GetSelectedValue());

		public static ArenaThemeDefinition[] GetDefinitions()
		{
			return Definitions;
		}

		public static string GetSelectedValue()
		{
#if UNITY_EDITOR
			if (Application.isPlaying && !editorDefaultApplied)
			{
				editorDefaultApplied = true;
				PlayerPrefs.SetString(PreferenceKey, DefaultTheme);
				PlayerPrefs.Save();
				return DefaultTheme;
			}
#endif
			var rawValue = PlayerPrefs.GetString(PreferenceKey, DefaultTheme);
			return GetDefinition(rawValue).PreferenceValue;
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
			if (string.Equals(preferenceValue, "classic", StringComparison.OrdinalIgnoreCase))
				preferenceValue = Vulcan;

			foreach (var definition in Definitions)
			{
				if (string.Equals(definition.PreferenceValue, preferenceValue, StringComparison.OrdinalIgnoreCase))
					return definition;
			}

			return Definitions[0];
		}
	}
}
