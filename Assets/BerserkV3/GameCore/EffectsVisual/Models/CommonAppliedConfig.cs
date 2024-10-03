using UnityEngine;

namespace BerserkV3.GameCore.EffectsVisual.Models
{
	[CreateAssetMenu(fileName = nameof(CommonAppliedConfig), menuName = "Berserk/GameCore/EffectVisuals/" + nameof(CommonAppliedConfig))]
	public class CommonAppliedConfig : ScriptableObject
	{
		[SerializeField] private float applicationDelayS;
		[SerializeField] private float expirationDelayS;

		public int ApplicationDelayMs => (int)(applicationDelayS * 1000);
		public int ExpirationDelayMs => (int)(expirationDelayS * 1000);
	}
}