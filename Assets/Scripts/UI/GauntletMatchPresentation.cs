namespace UI
{
	public static class GauntletMatchPresentation
	{
		private static string forcedArenaTheme;

		public static bool ForceCompact { get; private set; }

		public static bool IsActive => ForceCompact || !string.IsNullOrWhiteSpace(forcedArenaTheme);

		public static string ForcedArenaTheme => forcedArenaTheme;

		public static void Activate(string arenaTheme)
		{
			ForceCompact = true;
			forcedArenaTheme = string.IsNullOrWhiteSpace(arenaTheme)
				? null
				: arenaTheme;
		}

		public static void Clear()
		{
			ForceCompact = false;
			forcedArenaTheme = null;
		}
	}
}
