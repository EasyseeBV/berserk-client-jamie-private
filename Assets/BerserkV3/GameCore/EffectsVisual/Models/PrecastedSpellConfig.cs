using Berserk.Shared.Data.Enums;
using Newtonsoft.Json;
using UnityEngine;

namespace BerserkV3.GameCore.EffectsVisual.Models
{
	[CreateAssetMenu(fileName = nameof(PrecastedSpellConfig), menuName = "Berserk/GameCore/EffectVisuals/" + nameof(PrecastedSpellConfig))]
	public class PrecastedSpellConfig : ScriptableObject
	{
		[SerializeField] private EffectVisualKeyword visualKeyword;
		
		[SerializeField] private VfxKeyword vfxPrecastKeyword;

		[SerializeField] private VfxKeyword vfxImpactKeyword;
		[SerializeField] private float vfxImpactDelayS = -1;

		[SerializeField] private VfxKeyword vfxSecondEffectKeyword = VfxKeyword.None;
		[SerializeField] private float vfxSecondEffectDelayS = -1;
		
		[SerializeField] private VfxKeyword vfxThirdEffectKeyword = VfxKeyword.None;
		[SerializeField] private float vfxThirdEffectDelayS = -1;
		
		public EffectVisualKeyword VisualKeyword => visualKeyword;
		
		public VfxKeyword VFXPrecastKeyword => vfxPrecastKeyword;

		public VfxKeyword VFXImpactKeyword => vfxImpactKeyword;
		public float VFXImpactDelayS => vfxImpactDelayS;

		public VfxKeyword VFXSecondEffectKeyword => vfxSecondEffectKeyword;
		public float VFXSecondEffectDelayS => vfxSecondEffectDelayS;

		public VfxKeyword VFXThirdEffectKeyword => vfxThirdEffectKeyword;
		public float VFXThirdEffectDelayS => vfxThirdEffectDelayS;

		public override string ToString()
		{
			return $"{nameof(PrecastedSpellConfig)} {JsonConvert.SerializeObject(this)}";
		}
	}
}