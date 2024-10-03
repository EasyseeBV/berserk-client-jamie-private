using UnityEngine;

namespace BerserkV3.GameCore.TooltipPopup
{
	public class TooltipContainer : MonoBehaviour
	{
		[SerializeField] private RectTransform tooltipContainer;

		public RectTransform Container => tooltipContainer;
	}
}