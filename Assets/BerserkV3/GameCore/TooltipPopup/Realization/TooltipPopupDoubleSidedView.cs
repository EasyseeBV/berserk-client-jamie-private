using UnityEngine;

namespace BerserkV3.GameCore.TooltipPopup
{
	public class TooltipPopupDoubleSidedView : MonoBehaviour, ITooltipPopupDoubleSidedView
	{
		[SerializeField] private TooltipPopupView gainedPopupView;
		[SerializeField] private TooltipPopupView innatePopupView;

		public ITooltipPopupView GainedPopupView => gainedPopupView;
		public ITooltipPopupView InnatePopupView => innatePopupView;
	}
}