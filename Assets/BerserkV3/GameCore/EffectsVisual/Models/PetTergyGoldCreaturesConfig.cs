using UnityEngine;

namespace BerserkV3.GameCore.EffectsVisual.Models
{
	[CreateAssetMenu(fileName = nameof(PetTergyGoldCreaturesConfig),
		menuName = "Berserk/GameCore/EffectVisuals/" + nameof(PetTergyGoldCreaturesConfig))]
	public class PetTergyGoldCreaturesConfig : ScriptableObject
	{
		[SerializeField] private float vfxFieldDelayS = -1f;
		[SerializeField] private Vector3 vfxFieldPosition = Vector3.zero;

		[SerializeField] private float cardRaiseDelayS = .5f;
		[SerializeField] private Vector3 cardRaiseOffset = Vector3.up;
		[SerializeField] private Vector3 cardBurnOffset = Vector3.forward;

		[SerializeField] private float cardMoveDelayS = 1f;
		[SerializeField] private Vector3 cardMoveOffset = Vector3.forward;

		[SerializeField] private float projectileAppearDelayS = .6f;
		[SerializeField] private float projectileThrowDelayS = .5f;
		[SerializeField] private Vector3 projectileStartPosition = Vector3.up;

		[SerializeField] private float vfxExplosionDelayS = -1f;

		public float VfxFieldDelayS => vfxFieldDelayS;
		public Vector3 VFXFieldPosition => vfxFieldPosition;

		public float CardRaiseDelayS => cardRaiseDelayS;
		public Vector3 CardRaiseOffset => cardRaiseOffset;
		public Vector3 CardBurnOffset => cardBurnOffset;

		public float CardMoveDelayS => cardMoveDelayS;
		public Vector3 CardMoveOffset => cardMoveOffset;

		public float ProjectileAppearDelayS => projectileAppearDelayS;
		public float ProjectileThrowDelayS => projectileThrowDelayS;
		public Vector3 ProjectileStartPosition => projectileStartPosition;

		public float VfxExplosionDelayS => vfxExplosionDelayS;
	}
}