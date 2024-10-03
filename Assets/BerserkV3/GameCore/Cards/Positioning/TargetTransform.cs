using UnityEngine;

namespace BerserkV3.GameCore.Cards
{
	public readonly struct TargetTransform
	{
		public Vector3 Position { get; }
		public Quaternion Rotation { get; }

		public TargetTransform(Vector3 position, Quaternion rotation)
		{
			Rotation = rotation;
			Position = position;
		}
		public TargetTransform(Vector3 position)
		{
			Rotation = Quaternion.identity;
			Position = position;
		}
	}
}