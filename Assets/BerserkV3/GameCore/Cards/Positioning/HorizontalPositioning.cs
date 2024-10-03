using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BerserkV3.GameCore.Cards
{
	public class HorizontalPositioning : IHorizontalPositioning
	{
		public IEnumerable<TargetTransform> CalculatePositions(GroupPositioningOptions options)
		{
			var objectCount = Math.Abs(options.ObjectCount);
			if (objectCount == 0)
				return Array.Empty<TargetTransform>();

			var objSize = options.ObjectSize;
			var totalWidth = (objectCount * objSize) + ((objectCount - 1) * options.Space);
			var halfWidth = totalWidth / 2f;
			var position = options.Alignment switch
			{
				TextAlignment.Center => options.Center + Vector3.left * (halfWidth - objSize/2f),
				TextAlignment.Left => options.Center + Vector3.right * (objSize/2f),
				TextAlignment.Right => options.Center + Vector3.left * (totalWidth - objSize/2f),
				_ => options.Center
			};

			return Enumerable.Range(0, objectCount).Select(_ =>
			{
				var point = new TargetTransform(position);
				position.x += objSize + options.Space;
				return point;
			}).ToArray();
		}
	}
}