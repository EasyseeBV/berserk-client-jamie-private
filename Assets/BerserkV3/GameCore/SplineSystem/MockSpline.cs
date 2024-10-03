using BerserkV3.GameCore.Cards;
using UnityEngine;

namespace BerserkV3.GameCore.SplineSystem
{
	public class MockSpline : MonoBehaviour, ISpline
	{
		[SerializeField] private SplineType _splineType;
		public SplineType SplineType => _splineType;
		public int SegmentCount { get; set; } = 6;

		public TargetTransform Evaluate(float time)
		{
			return new TargetTransform(transform.localPosition, transform.localRotation);
		}
	}
}