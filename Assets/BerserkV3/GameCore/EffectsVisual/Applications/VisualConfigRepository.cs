using System;
using System.Linq;
using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.EffectsVisual.Models;
using UnityEngine;

namespace BerserkV3.GameCore.EffectsVisual.Applications
{
	public interface IVisualConfigRepository
	{
		public CommonAppliedConfig CommonApplied { get; }
		public CommonPrecastedConfig CommonPrecasted { get; }
		public ProjectileSpellConfig IceBolt { get; }
		public ProjectileSpellConfig AdventurerLight { get; }
		public AdventurerWaterRainConfig AdventurerWaterRain { get; }
		public AdventurerFireConfig AdventurerFire { get; }
		public PetTergyGoldCreaturesConfig PetTergyGoldCreatures { get; }

		PrecastedSpellConfig GetBasicPrecastedConfig(EffectVisualKeyword visualKeyword);
	} 
	
	[CreateAssetMenu(fileName = nameof(VisualConfigRepository), menuName = "Berserk/GameCore/EffectVisuals/" + nameof(VisualConfigRepository))]
	public class VisualConfigRepository : ScriptableObject, IVisualConfigRepository
	{
		[SerializeField] private CommonAppliedConfig commonApplied;
		[SerializeField] private CommonPrecastedConfig commonPrecasted;
		[SerializeField] private ProjectileSpellConfig iceBolt;
		[SerializeField] private ProjectileSpellConfig adventurerLight;
		[SerializeField] private PrecastedSpellConfig[] basicPrecastedConfigs;
		[SerializeField] private AdventurerWaterRainConfig adventurerWaterRain;
		[SerializeField] private AdventurerFireConfig adventurerFire;
		[SerializeField] private PetTergyGoldCreaturesConfig petTergyGoldCreatures;

		public CommonAppliedConfig CommonApplied => commonApplied;
		public CommonPrecastedConfig CommonPrecasted => commonPrecasted;
		public ProjectileSpellConfig IceBolt => iceBolt;
		public ProjectileSpellConfig AdventurerLight => adventurerLight;
		public AdventurerWaterRainConfig AdventurerWaterRain => adventurerWaterRain;
		public AdventurerFireConfig AdventurerFire => adventurerFire;
		public PetTergyGoldCreaturesConfig PetTergyGoldCreatures => petTergyGoldCreatures;

		public PrecastedSpellConfig GetBasicPrecastedConfig(EffectVisualKeyword visualKeyword)
		{
			var config = basicPrecastedConfigs.FirstOrDefault(cfg => cfg.VisualKeyword == visualKeyword);
			if (config == null)
				throw new ArgumentException(
					$"{nameof(PrecastedSpellConfig)} for visual {visualKeyword} was not found!");

			return config;
		}
	}
}