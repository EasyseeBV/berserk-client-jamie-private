using System.Collections.Generic;
using Berserk.Shared.GameCore.Abstraction;
using BerserkV3.GameCore.SplineSystem;
using BerserkV3.Startup.Authorization;

namespace BerserkV3.GameCore.Cards
{
	public class OpponentHandCardsPositioning : IOpponentHandCardsPositioning
	{
		private readonly ISpline spline;
		private readonly IGameContext gameContext;

		public OpponentHandCardsPositioning(
			ISplineStorage storage,
			IGameContext gameContext)
		{
			spline = storage.Get(SplineType.HandOpponent);
			this.gameContext = gameContext;
		}

		public IEnumerable<TargetTransform> CalculatePositions(int objectsCount)
		{
			var player = gameContext.PlayerRepository.GetOpposite(User.Id);
			var offset = (player.RuntimeData.HandCount - objectsCount) / 2f;
			spline.SegmentCount = player.RuntimeData.HandCount;
			
			for (var i = 0; i < objectsCount; ++i)
			{
				yield return spline.Evaluate(offset + i);
			}
		}
	}
}