using UnityEngine;

namespace BerserkV3.GameCore.Cards
{
	[CreateAssetMenu(fileName = "RuntimeLayoutSettings", menuName = "BerserkV3/RuntimeLayoutSettings")]
	public class RuntimeLayoutSettings : ScriptableObject
	{
		[SerializeField] private Color buffedColor;
		[SerializeField] private Color debuffedColor;
		[SerializeField] private Color defaultTextColor;

		public Color BuffedColor => buffedColor;
		public Color DebuffedColor => debuffedColor;
		public Color DefaultTextColor => defaultTextColor;
	}
}