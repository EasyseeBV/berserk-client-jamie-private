using UnityEngine;

namespace RR.Core.Extensions
{
	public static class TransformExtensions
	{
		public static void CopyPosition(this Transform transform, Transform other)
		{
			transform.position = other.position;
		}

		public static void CopyLocalPosition(this Transform transform, Transform other)
		{
			transform.localPosition = other.localPosition;
		}

		public static void CopyRotation(this Transform transform, Transform other)
		{
			transform.rotation = other.rotation;
		}

		public static void CopyLocalRotation(this Transform transform, Transform other)
		{
			transform.localRotation = other.localRotation;
		}

		public static void CopyLocalScale(this Transform transform, Transform other)
		{
			transform.localScale = other.localScale;
		}

		public static void CopyAll(this Transform transform, Transform other)
		{
			transform.CopyPosition(other);
			transform.CopyRotation(other);
			transform.CopyLocalScale(other);
			transform.CopyLocalPosition(other);
			transform.CopyLocalRotation(other);
		}

		public static Transform ResetLocalPosition(this Transform transform)
		{
			transform.localPosition = Vector3.zero;
			return transform;
		}

		public static Transform ResetLocalRotation(this Transform transform)
		{
			transform.localRotation = Quaternion.identity;
			return transform;
		}

		public static Transform ResetLocalScale(this Transform transform)
		{
			transform.localScale = Vector3.zero;
			return transform;
		}

		public static Transform ResetPosition(this Transform transform)
		{
			transform.position = Vector3.zero;
			return transform;
		}

		public static Transform ResetRotation(this Transform transform)
		{
			transform.rotation = Quaternion.identity;
			return transform;
		}
	}
}