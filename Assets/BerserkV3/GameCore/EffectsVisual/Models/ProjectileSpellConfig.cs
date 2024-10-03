using Berserk.Shared.Data.Enums;
using Newtonsoft.Json;
using UnityEngine;

namespace BerserkV3.GameCore.EffectsVisual.Models
{
	[CreateAssetMenu(fileName = nameof(ProjectileSpellConfig), menuName = "Berserk/GameCore/EffectVisuals/" + nameof(ProjectileSpellConfig))]
	public class ProjectileSpellConfig : PrecastedSpellConfig
	{
		[SerializeField] private VfxKeyword vfxProjectileKeyword;
		[SerializeField] private float projectileDurationS;
		
		public VfxKeyword VFXProjectileKeyword => vfxProjectileKeyword;
		public float ProjectileDurationS => projectileDurationS;

		public override string ToString()
		{
			return $"{nameof(ProjectileSpellConfig)} {JsonConvert.SerializeObject(this)}";
		}
	}
}