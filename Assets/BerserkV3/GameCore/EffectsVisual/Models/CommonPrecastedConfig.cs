using Newtonsoft.Json;
using UnityEngine;

namespace BerserkV3.GameCore.EffectsVisual.Models
{
	[CreateAssetMenu(fileName = nameof(CommonPrecastedConfig), menuName = "Berserk/GameCore/EffectVisuals/" + nameof(CommonPrecastedConfig))]
	public class CommonPrecastedConfig : ScriptableObject
	{
		[SerializeField] private float castDelayS = 0.5f;
		[SerializeField] private float castDurationS = 1f;
		
		public float CastDelayS => castDelayS;
		public float CastDurationS => castDurationS;

		public override string ToString()
		{
			return $"{nameof(PrecastedSpellConfig)} {JsonConvert.SerializeObject(this)}";
		}
	}
}