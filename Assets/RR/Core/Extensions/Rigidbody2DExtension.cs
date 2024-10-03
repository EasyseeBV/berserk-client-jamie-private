using UnityEngine;

namespace RR.Core.Extensions
{
	public static class Rigidbody2DExtension
	{
		public static void AddExplosionForce(this Rigidbody2D body, float explosionForce, Vector3 explosionPosition, float explosionRadius)
		{
			var dir = (body.transform.position - explosionPosition);
			var wOff = 1 - (dir.magnitude / explosionRadius);
			body.AddForce(dir.normalized * (wOff <= 0f ? 0f : explosionForce) * wOff);
		}

		public static void AddExplosionForce(this Rigidbody2D body, float explosionForce, Vector3 explosionPosition, float explosionRadius, float upliftModifier)
		{
			var dir = (body.transform.position - explosionPosition);
			var wOff = 1 - (dir.magnitude / explosionRadius);
			var baseForce = dir.normalized * (wOff <= 0f ? 0f : explosionForce) * wOff;
			body.AddForce(baseForce);

			var upliftWOff = 1 - upliftModifier / explosionRadius;
			Vector3 upliftForce = Vector2.up * explosionForce * upliftWOff;
			body.AddForce(upliftForce);
		}
	}
}