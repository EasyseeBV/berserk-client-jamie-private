using BerserkV3.Common.Network;
using BerserkV3.Startup.Authorization;
using UnityEngine;

namespace UI
{
	public static class GauntletProgressSettings
	{
		private const string KeyPrefix = "JamieGauntletProgressV1";

		public static readonly string[] BotDeckIds =
		{
			"bot_gauntlet_1",
			"bot_gauntlet_2",
			"bot_gauntlet_3",
			"bot_gauntlet_4",
			"bot_gauntlet_5"
		};

		public static int ChampionCount => BotDeckIds.Length;

		public static int GetCompletedCount()
		{
			return Mathf.Clamp(PlayerPrefs.GetInt(GetScopedKey("CompletedCount"), 0), 0, ChampionCount);
		}

		public static bool IsUnlocked(int championIndex)
		{
			return championIndex >= 0 && championIndex < ChampionCount && championIndex <= Mathf.Min(GetCompletedCount(), ChampionCount - 1);
		}

		public static bool IsCleared(int championIndex)
		{
			return championIndex >= 0 && championIndex < GetCompletedCount();
		}

		public static bool IsFullyCleared()
		{
			return GetCompletedCount() >= ChampionCount;
		}

		public static int GetNextChampionIndex()
		{
			return Mathf.Clamp(GetCompletedCount(), 0, ChampionCount - 1);
		}

		public static string GetMainCardProgressText()
		{
			return $"{GetCompletedCount()}/{ChampionCount} Champions Cleared";
		}

		public static string GetStarTrackText()
		{
			var completed = GetCompletedCount();
			var chars = new char[ChampionCount];
			for (var i = 0; i < ChampionCount; i++)
				chars[i] = i < completed ? '★' : '☆';

			return new string(chars);
		}

		public static bool IsGauntletDeckId(string botDeckId)
		{
			return TryGetChampionIndex(botDeckId, out _);
		}

		public static bool TryGetChampionIndex(string botDeckId, out int championIndex)
		{
			championIndex = System.Array.IndexOf(BotDeckIds, botDeckId);
			return championIndex >= 0;
		}

		public static void RegisterVictory(string botDeckId)
		{
			if (!TryGetChampionIndex(botDeckId, out var championIndex))
				return;

			var completedCount = GetCompletedCount();
			var nextCompletedCount = Mathf.Max(completedCount, championIndex + 1);
			if (nextCompletedCount == completedCount)
				return;

			PlayerPrefs.SetInt(GetScopedKey("CompletedCount"), nextCompletedCount);
			PlayerPrefs.Save();
		}

		private static string GetScopedKey(string suffix)
		{
			var environment = EnvironmentSwitcher.CurrentEnvironment.ToString();
			var userId = string.IsNullOrWhiteSpace(User.Id) ? "anonymous" : User.Id;
			return $"{KeyPrefix}_{environment}_{userId}_{suffix}";
		}
	}
}
