using UnityEngine;

namespace RR.Core.Extensions
{
	public static class QuaternionExtensions
	{
		/// <summary>
		/// Clamps rotation to be in bounds.
		/// [-180, 180] are maximum bounds.
		/// </summary>
		public static Quaternion ClampRotation(this Quaternion q, Vector3 bounds)
		{
			static float ConvertToEditorEuler(float axisValue) 
				=> axisValue % 360 > 180 ? axisValue - 360 : axisValue;

			var euler = q.eulerAngles;

			euler = new Vector3(
				Mathf.Clamp(ConvertToEditorEuler(euler.x), -bounds.x, bounds.x),
				Mathf.Clamp(ConvertToEditorEuler(euler.y), -bounds.y, bounds.y),
				Mathf.Clamp(ConvertToEditorEuler(euler.z), -bounds.z, bounds.z));

			return Quaternion.Euler(euler);
		}
	}
}