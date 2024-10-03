using RR.Core.Extensions;

namespace BerserkV3.GameCore.Cards
{
	public static class PositioningUtils
	{
		public static int GetVirtualRelativePosition(float position, float[] neighborsPositions)
		{
			var virtualPositionX = neighborsPositions.IndexOf(n => position < n);
			return virtualPositionX < 0 ? neighborsPositions.Length : virtualPositionX;
		}
	}
}