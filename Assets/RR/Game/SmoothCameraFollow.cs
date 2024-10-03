using Sirenix.OdinInspector;
using UnityEngine;

namespace RR.Game
{
	public class SmoothCameraFollow : MonoBehaviour
	{
		public float SmoothSpeed = 3;
		public Vector3 PositionOffset;
		public Vector3 LookAtOffset;

		public Transform Target;

		private void LateUpdate()
		{
			if (!Target)
				return;

			transform.position = Vector3.Slerp(transform.position, Target.position + PositionOffset, SmoothSpeed * Time.deltaTime);

			var lookOnLook = Quaternion.LookRotation(Target.position + LookAtOffset - transform.position);
			transform.rotation = Quaternion.Slerp(transform.rotation, lookOnLook, SmoothSpeed * Time.deltaTime);
		}

		[Button]
		public void GrabPositionFromCurrentTransformPos()
		{
			PositionOffset = transform.position;
			if (Target)
				LookAtOffset = Target.position - transform.position;
		}
	}
}
