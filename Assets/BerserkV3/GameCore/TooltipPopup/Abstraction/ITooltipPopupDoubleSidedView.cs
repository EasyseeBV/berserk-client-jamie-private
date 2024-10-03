namespace BerserkV3.GameCore.TooltipPopup
{
	public interface ITooltipPopupDoubleSidedView
	{
		ITooltipPopupView GainedPopupView { get; }
		ITooltipPopupView InnatePopupView { get; }
	}
}