using System.Collections.Generic;
using UnityEngine;

namespace BerserkV3.GameCore.Cards
{
	public struct GridPositioningOptions
	{
		public Vector3 StartPoint { get; }
		public int ObjectCount { get; }
		public int Column { get; set; }
		public float ObjectWidth { get; set; }
		public float ObjectHeight { get; set; }
		public float SpaceHorizontal { get; set; }
		public float SpaceVertical { get; set; }
		public TextAlignment Alignment  { get; set; }
		
		public GridPositioningOptions(Vector3 startPoint, int objectCount)
		{
			StartPoint = startPoint;
			ObjectCount = objectCount;
			Column = 10;
			ObjectWidth = 1f;
			ObjectHeight = 1f;
			SpaceHorizontal = 0;
			SpaceVertical = 0;
			Alignment = TextAlignment.Left;
		}
	}
	
	public interface IGridPositioning
	{
		IEnumerable<TargetTransform> CalculatePositions(GridPositioningOptions options);
	}
}