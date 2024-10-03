using BerserkV3.GameCore.Cards;

namespace BerserkV3.GameCore.SplineSystem
{
	public interface ISpline
	{
		public SplineType SplineType { get; }
		public int SegmentCount { get; set; }
		/// <summary>
		/// Return target created from spline
		/// </summary>
		/// <param name="time">0-1 space</param>
		TargetTransform Evaluate(float time);
	}
}