using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BerserkV3.GameCore.Cards
{
	/// <summary>
	/// WIP, maybe need improve
	/// </summary>
	public class GridPositioning : IGridPositioning
	{
		private readonly IHorizontalPositioning horizontalPositioning;
		public GridPositioning(IHorizontalPositioning horizontalPositioning)
		{
			this.horizontalPositioning = horizontalPositioning;
		}

		public IEnumerable<TargetTransform> CalculatePositions(GridPositioningOptions options)
		{
			var objectCount = Mathf.Abs(options.ObjectCount);
			if (objectCount == 0)
				return Array.Empty<TargetTransform>();

			var columns = Mathf.Max(1, options.Column);
			var position = options.StartPoint;
			return Enumerable.Range(0, Mathf.CeilToInt(objectCount / (float) columns)).SelectMany(_ =>
			{
				var rowObjectCount = columns;
				if (rowObjectCount > objectCount)
					rowObjectCount = objectCount;

				var rowOptions = new GroupPositioningOptions(position, rowObjectCount, options.ObjectWidth)
				{
					Alignment = options.Alignment,
					Space = options.SpaceHorizontal
				};

				var positions = horizontalPositioning.CalculatePositions(rowOptions);
				position.y -= options.ObjectHeight + options.SpaceVertical;
				objectCount -= rowObjectCount;
				
				return positions;
			});
		}
	}
}