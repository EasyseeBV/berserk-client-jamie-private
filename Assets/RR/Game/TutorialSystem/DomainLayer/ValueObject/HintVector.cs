using UnityEngine;

namespace RR.Game.TutorialSystem.Domain
{
	public class HintVector
	{
		public static HintVector Zero => new(Vector3.zero, Vector2.zero);
		public Vector2 SizeDelta { get; }
		public Vector3 Position { get; }

		public bool IsAutoSize => SizeDelta == Vector2.zero;

		public HintVector(Vector3 offset, Vector2 fixedRect)
		{
			Position = offset;
			SizeDelta = fixedRect;
		}
	}
}