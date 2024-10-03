using Berserk.Shared.Data.Enums;
using UnityEngine;

namespace BerserkV3.GameCore.EffectsVisual.Models
{
	[CreateAssetMenu(fileName = nameof(AdventurerFireConfig), menuName = "Berserk/GameCore/EffectVisuals/" + nameof(AdventurerFireConfig))]
	public class AdventurerFireConfig : ScriptableObject
	{
		[SerializeField] private VfxKeyword vfxHero;
		[SerializeField] private float vfxHeroDelayS = -1;

		[SerializeField] private VfxKeyword vfxImpact;
		[SerializeField] private float vfxImpactDelayS = -1;

		public VfxKeyword VFXHero => vfxHero;
		public float VFXHeroDelayS => vfxHeroDelayS;

		public VfxKeyword VFXImpact => vfxImpact;
		public float VFXImpactDelayS => vfxImpactDelayS;
	}
}