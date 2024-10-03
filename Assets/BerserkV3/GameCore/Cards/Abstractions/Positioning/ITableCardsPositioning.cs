using System.Collections.Generic;
using Berserk.Shared.Data.Enums;

namespace BerserkV3.GameCore.Cards
{
	public interface ITableCardsPositioning : IObjectsPositioning
	{
		IEnumerable<TargetTransform> CalculatePositions(int objectsCount, float width, float height, Owner owner = Owner.None);
	}
}