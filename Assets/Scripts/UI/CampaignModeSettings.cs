using System.Collections.Generic;
using Berserk.Shared.Data.Enums;

namespace UI
{
	public enum CampaignStageVisualType
	{
		Regular,
		Special,
		Puzzle,
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
			Quadrant quadrant,
			string id,
			string displayName,
			string subtitle,
			string themeName,
			string rewardText,
			string previewTextureResource,
			string emblemTextureResource,
			string topIconResource,
			bool isLocked,
			IReadOnlyList<CampaignStageDefinition> stages)
		{
			Quadrant = quadrant;
			Id = id;
			DisplayName = displayName;
			Subtitle = subtitle;
			ThemeName = themeName;
			RewardText = rewardText;
			PreviewTextureResource = previewTextureResource;
			EmblemTextureResource = emblemTextureResource;
			TopIconTextureResource = topIconResource;
			IsLocked = isLocked;
			Stages = stages;
		}

		public Quadrant Quadrant { get; }
		public string Id { get; }
		public string DisplayName { get; }
		public string Subtitle { get; }
		public string ThemeName { get; }
		public string RewardText { get; }
		public string PreviewTextureResource { get; }
		public string EmblemTextureResource { get; }
		public string TopIconTextureResource { get; }
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
				Quadrant.Boreas,
				"boreas",
				"BOREAS",
				"Frozen beasts, siege trials, and the Frost King.",
				"Boreas Arena",
				"8 encounters  •  5 trials  •  2 elites  •  1 epic boss",
				"UI/Campaign/boreas_bg",
				"UI/Campaign/boreas_emblem",
				"UI/Campaign/boreas_top_banner",
				false,
				CreateStandardStageSet("Frost"));
		}

		private static CampaignQuadrantDefinition CreateArcadia()
		{
			return new CampaignQuadrantDefinition(
				Quadrant.Arcadia,
				"arcadia",
				"ARCADIA",
				"Wildwood guardians, beast swarms, and the Blighted Heart.",
				"Arcadia Arena",
				"8 encounters  •  5 trials  •  2 elites  •  1 epic boss",
				"UI/Campaign/arcadia_bg",
				"UI/Campaign/arcadia_emblem",
				"UI/Campaign/arcadia_top_banner",
				false,
				CreateStandardStageSet("Wild"));
		}

		private static CampaignQuadrantDefinition CreateNotus()
		{
			return new CampaignQuadrantDefinition(
				Quadrant.Notus,
				"notus",
				"NOTUS",
				"Desert fire, puzzle duels, and the Sun Pharaoh.",
				"Notus Arena",
				"8 encounters  •  5 trials  •  2 elites  •  1 epic boss",
				"UI/Campaign/notus_bg",
				"UI/Campaign/notus_emblem",
				"UI/Campaign/notus_top_banner",
				false,
				CreateStandardStageSet("Flame"));
		}

		private static CampaignQuadrantDefinition CreateHades()
		{
			return new CampaignQuadrantDefinition(
				Quadrant.Hades,
				"hades",
				"HADES",
				"Death magic, attrition battles, and the Lord of the Dead.",
				"Hades Arena",
				"8 encounters  •  5 trials  •  2 elites  •  1 epic boss",
				"UI/Campaign/hades_bg",
				"UI/Campaign/hades_emblem",
				"UI/Campaign/hades_top_banner",
				false,
				CreateStandardStageSet("Death"));
		}

		private static CampaignQuadrantDefinition CreateVulcanCity()
		{
			return new CampaignQuadrantDefinition(
				Quadrant.Vulcan_City,
				"vulcan_city",
				"VULCAN CITY",
				"Three final fights through the forge district, cinder arena, and Vulcan's throne.",
				"Colosseum Arena",
				"3 finale fights  •  forge siege  •  cinder arena  •  Vulcan throne",
				"UI/Campaign/vulcan_city_bg",
				"UI/Campaign/vulcan_city_emblem",
				"UI/Campaign/vulcan_city_top_banner",
				true,
				new[]
				{
					new CampaignStageDefinition(1, "Forge Master", "Break the molten gate", CampaignStageVisualType.Elite),
					new CampaignStageDefinition(2, "The Champion", "Survive the cinder arena", CampaignStageVisualType.Elite),
					new CampaignStageDefinition(3, "Vulcan", "Final throne confrontation", CampaignStageVisualType.Epic)
				});
		}

		private static CampaignStageDefinition[] CreateStandardStageSet(string prefix)
		{
			return new[]
			{
				new CampaignStageDefinition(1, $"{prefix} Trial I", "Regular encounter", CampaignStageVisualType.Regular),
				new CampaignStageDefinition(2, $"{prefix} Trial II",
					prefix == "Frost" || prefix == "Death" ? "Puzzle duel" : "Regular encounter",
					prefix == "Frost" || prefix == "Death" ? CampaignStageVisualType.Puzzle : CampaignStageVisualType.Regular),
				new CampaignStageDefinition(3, $"{prefix} Trial III",
					prefix == "Wild" ? "Special tempo encounter" : "Regular encounter",
					prefix == "Wild" ? CampaignStageVisualType.Special : CampaignStageVisualType.Regular),
				new CampaignStageDefinition(4, $"{prefix} Trial IV",
					prefix == "Flame" ? "Puzzle duel" : prefix == "Frost" || prefix == "Death" ? "Special rule encounter" : "Regular encounter",
					prefix == "Flame" ? CampaignStageVisualType.Puzzle : prefix == "Frost" || prefix == "Death" ? CampaignStageVisualType.Special : CampaignStageVisualType.Regular),
				new CampaignStageDefinition(5, $"{prefix} Trial V",
					prefix == "Wild" ? "Puzzle duel" : "Regular encounter",
					prefix == "Wild" ? CampaignStageVisualType.Puzzle : CampaignStageVisualType.Regular),
				new CampaignStageDefinition(6, $"{prefix} Elite I", "Elite encounter", CampaignStageVisualType.Elite),
				new CampaignStageDefinition(7, $"{prefix} Elite II", "Elite encounter", CampaignStageVisualType.Elite),
				new CampaignStageDefinition(8, $"{prefix} Epic Boss", "Epic boss encounter", CampaignStageVisualType.Epic)
			};
		}
	}
}
