using UnityEngine;

namespace BerserkV3.GameCore.Cards
{
	[CreateAssetMenu(fileName = "GlowSettings", menuName = "BerserkV3/GlowSettings")]
	public class GlowSettings : ScriptableObject
	{
		[SerializeField] private Color selfTargetingColor;
		[SerializeField] private Color selfSelectingColor;

		[SerializeField] private Color opponentTargetingColor;
		[SerializeField] private Color opponentSelectingColor;

		public Color SelfTargetingColor => selfTargetingColor;
		public Color SelfSelectionColor => selfSelectingColor;
		public Color OpponentTargetingColor => opponentTargetingColor;
		public Color OpponentSelectionColor => opponentSelectingColor;
	}
}