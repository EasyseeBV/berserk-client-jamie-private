using System.Collections.Generic;

namespace BerserkV3.GameCore.Cards
{
	public interface IObjectsPositioning
	{
		IEnumerable<TargetTransform>  CalculatePositions(int objectsCount);
	}
}