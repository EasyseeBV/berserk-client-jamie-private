using Berserk.Shared.Data.Enums;
using UnityEngine;

namespace BerserkV3.GameCore.EffectsVisual.Models
{
	[CreateAssetMenu(fileName = nameof(AdventurerWaterRainConfig), menuName = "Berserk/GameCore/EffectVisuals/" + nameof(AdventurerWaterRainConfig))]
	public class AdventurerWaterRainConfig : ScriptableObject
	{
		[SerializeField] private Vector3 rainSpawnPosition;
		[SerializeField] private float vfxKeywordDefaultImpactDelay;
		[SerializeField] private VfxKeyword vfxKeywordDefaultImpact;

		public Vector3 RainSpawnPosition => rainSpawnPosition;
		public VfxKeyword VfxKeywordDefaultImpact => vfxKeywordDefaultImpact;

		public float VfxKeywordDefaultImpactDelay => vfxKeywordDefaultImpactDelay;
	}
}