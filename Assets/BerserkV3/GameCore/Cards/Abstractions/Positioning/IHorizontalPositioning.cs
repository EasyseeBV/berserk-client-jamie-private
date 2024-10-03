using System.Collections.Generic;
using UnityEngine;

namespace BerserkV3.GameCore.Cards
{

	public struct GroupPositioningOptions
	{
		public Vector3 Center { get; }
		public int ObjectCount { get; }
		public float ObjectSize { get; }
		public float Space { get; set; }
		public TextAlignment Alignment  { get; set; }
		
		public GroupPositioningOptions(Vector3 center, int objectCount, float objectSize)
		{
			Center = center;
			ObjectCount = objectCount;
			ObjectSize = objectSize;
			Space = 0;
			Alignment = TextAlignment.Left;
		}
	}

	public interface IHorizontalPositioning
	{
		IEnumerable<TargetTransform> CalculatePositions(GroupPositioningOptions options);
	}
}