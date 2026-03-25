using Berserk.Shared.GameCore.Abstraction;
using BerserkV3.GameCore.Cards;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using UnityEngine;

namespace BerserkV3.GameCore.SplineSystem
{
	public class HandSpline : MonoBehaviour, ISpline
	{
		[SerializeField] private float arcSize = 8;
		[SerializeField] private Vector2 arcScale = new Vector2(300, 200);
		[SerializeField] private SplineType splineType = SplineType.HandSelf;
		[SerializeField] private Transform splineCenter;
	#if UNITY_EDITOR
		[SerializeField] private Transform[] debugPlaceTargets;
	#endif

		public SplineType SplineType => splineType;
		public int SegmentCount { get; set; } = 6;
		private IGameContext gameContext;

		public float GetArcSize() => arcSize;
		public void SetArcSize(float value) => arcSize = value;
		public Vector2 GetArcScale() => arcScale;
		public void SetArcScale(Vector2 value) => arcScale = value;
		
		public TargetTransform Evaluate(float relativePositionX)
		{
			if (splineType != SplineType.HandOpponent && splineType != SplineType.HandSelf)
			{
				RRLogger.Error($"[{GetType().Name.Red()}] Spline id is not Hand");
				return default;
			}
			
			var ownerFactor = splineType == SplineType.HandSelf ? 1 : -1;
			var angle = (-relativePositionX + (SegmentCount - 1) / 2f) * arcSize;
			
			var x = -Mathf.Sin(angle * Mathf.PI / 180f) * arcScale.x;
			var y = ownerFactor * Mathf.Cos(angle * Mathf.PI / 180f) * arcScale.y;
				
			var position = new Vector3(x, y - (arcScale.y * ownerFactor), 0);
			var rotation = Quaternion.Euler(0, 0, ownerFactor * angle);
			
			return new TargetTransform(position, rotation);
		}

	#if UNITY_EDITOR
		private void OnValidate()
		{
			if (splineCenter == null)
				splineCenter = transform;
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			for (var i = 1; i < SegmentCount; i++)
			{
				var taget1 = Evaluate(i-1);
				var taget2 = Evaluate(i);
				if (debugPlaceTargets.Length > 0 && i - 1 < debugPlaceTargets.Length)
				{
					debugPlaceTargets[i-1].localPosition = taget1.Position;
					debugPlaceTargets[i-1].localRotation = taget1.Rotation;
				}
				Gizmos.DrawLine(splineCenter.localToWorldMatrix.MultiplyPoint3x4(taget1.Position)
				               ,splineCenter.localToWorldMatrix.MultiplyPoint3x4(taget2.Position));
				Gizmos.DrawSphere(splineCenter.localToWorldMatrix.MultiplyPoint3x4(taget1.Position),0.1f);
				if (i == SegmentCount - 1)
				{
					if (debugPlaceTargets.Length  > 0 && i < debugPlaceTargets.Length)
					{
						debugPlaceTargets[i].localPosition = taget2.Position;
						debugPlaceTargets[i].localRotation = taget2.Rotation;
					}
					Gizmos.DrawSphere(splineCenter.localToWorldMatrix.MultiplyPoint3x4(taget2.Position),0.1f);
				}
			}
		}
	#endif
	}
}
