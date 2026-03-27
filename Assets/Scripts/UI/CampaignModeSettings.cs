using System.Collections.Generic;

namespace UI
{
	public enum CampaignStageVisualType
	{
		Regular,
		Elite,
		Epic
	}

	public sealed class CampaignStageDefinition
	{
		public CampaignStageDefinition(int index, string title, string subtitle, CampaignStageVisualType visualType)
		{
			Index = index;
			Title = title;
			Subtitle = subtitle;
			VisualType = visualType;
		}

		public int Index { get; }
		public string Title { get; }
		public string Subtitle { get; }
		public CampaignStageVisualType VisualType { get; }
	}

	public sealed class CampaignQuadrantDefinition
	{
		public CampaignQuadrantDefinition(
			string id,
			string displayName,
			string subtitle,
			string themeName,
			string rewardText,
			string previewTextureResource,
			string emblemTextureResource,
			bool isLocked,
			IReadOnlyList<CampaignStageDefinition> stages)
		{
			Id = id;
			DisplayName = displayName;
			Subtitle = subtitle;
			ThemeName = themeName;
			RewardText = rewardText;
			PreviewTextureResource = previewTextureResource;
			EmblemTextureResource = emblemTextureResource;
			IsLocked = isLocked;
			Stages = stages;
		}

		public string Id { get; }
		public string DisplayName { get; }
		public string Subtitle { get; }
		public string ThemeName { get; }
		public string RewardText { get; }
		public string PreviewTextureResource { get; }
		public string EmblemTextureResource { get; }
		public bool IsLocked { get; }
		public IReadOnlyList<CampaignStageDefinition> Stages { get; }
	}

	public static class CampaignModeSettings
	{
		public static readonly IReadOnlyList<CampaignQuadrantDefinition> Quadrants = new[]
		{
			CreateBoreas(),
			CreateArcadia(),
			CreateNotus(),
			CreateHades(),
			CreateVulcanCity()
		};

		public static CampaignQuadrantDefinition GetQuadrant(string quadrantId)
		{
			foreach (var quadrant in Quadrants)
			{
				if (quadrant.Id == quadrantId)
					return quadrant;
			}

			return null;
		}

		private static CampaignQuadrantDefinition CreateBoreas()
		{
			return new CampaignQuadrantDefinition(
				"boreas",
				"BOREAS",
				"Frozen beasts, siege trials, and the Frost King.",
				"Boreas Arena",
				"Reward: Boreas Crest + 8 Bosses",
				"UI/Campaign/boreas_bg",
				"UI/Campaign/boreas_emblem",
				false,
				CreateStandardStageSet("Frost"));
		}

		private static CampaignQuadrantDefinition CreateArcadia()
		{
			return new CampaignQuadrantDefinition(
				"arcadia",
				"ARCADIA",
				"Wildwood guardians, beast swarms, and the Blighted Heart.",
				"Arcadia Arena",
				"Reward: Arcadia Crest + 8 Bosses",
				"UI/Campaign/arcadia_bg",
				"UI/Campaign/arcadia_emblem",
				false,
				CreateStandardStageSet("Wild"));
		}

		private static CampaignQuadrantDefinition CreateNotus()
		{
			return new CampaignQuadrantDefinition(
				"notus",
				"NOTUS",
				"Desert fire, puzzle duels, and the Sun Pharaoh.",
				"Notus Arena",
				"Reward: Notus Crest + 8 Bosses",
				"UI/Campaign/notus_bg",
				"UI/Campaign/notus_emblem",
				false,
				CreateStandardStageSet("Flame"));
		}

		private static CampaignQuadrantDefinition CreateHades()
		{
			return new CampaignQuadrantDefinition(
				"hades",
				"HADES",
				"Death magic, attrition battles, and the Lord of the Dead.",
				"Hades Arena",
				"Reward: Hades Crest + 8 Bosses",
				"UI/Campaign/hades_bg",
				"UI/Campaign/hades_emblem",
				false,
				CreateStandardStageSet("Death"));
		}

		private static CampaignQuadrantDefinition CreateVulcanCity()
		{
			return new CampaignQuadrantDefinition(
				"vulcan_city",
				"VULCAN CITY",
				"Three final trials against the forge elite.",
				"Colosseum Arena",
				"Unlock after all four quadrants are cleared",
				"UI/Campaign/vulcan_city_bg",
				"UI/Campaign/vulcan_city_emblem",
				true,
				new[]
				{
					new CampaignStageDefinition(1, "Forge Master", "Opening finale", CampaignStageVisualType.Elite),
					new CampaignStageDefinition(2, "The Champion", "Arena gauntlet", CampaignStageVisualType.Elite),
					new CampaignStageDefinition(3, "Vulcan", "Epic final boss", CampaignStageVisualType.Epic)
				});
		}

		private static CampaignStageDefinition[] CreateStandardStageSet(string prefix)
		{
			return new[]
			{
				new CampaignStageDefinition(1, $"{prefix} Trial I", "Regular encounter", CampaignStageVisualType.Regular),
				new CampaignStageDefinition(2, $"{prefix} Trial II", "Regular encounter", CampaignStageVisualType.Regular),
				new CampaignStageDefinition(3, $"{prefix} Trial III", "Regular encounter", CampaignStageVisualType.Regular),
				new CampaignStageDefinition(4, $"{prefix} Trial IV", "Regular encounter", CampaignStageVisualType.Regular),
				new CampaignStageDefinition(5, $"{prefix} Trial V", "Regular encounter", CampaignStageVisualType.Regular),
				new CampaignStageDefinition(6, $"{prefix} Elite I", "Elite encounter", CampaignStageVisualType.Elite),
				new CampaignStageDefinition(7, $"{prefix} Elite II", "Elite encounter", CampaignStageVisualType.Elite),
				new CampaignStageDefinition(8, $"{prefix} Epic Boss", "Epic boss encounter", CampaignStageVisualType.Epic)
			};
		}
	}
}
