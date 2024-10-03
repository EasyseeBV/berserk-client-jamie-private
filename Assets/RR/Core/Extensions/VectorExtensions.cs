using UnityEngine;

namespace RR.Core.Extensions
{
	public static class VectorExtensions
	{
		public static Vector3 Abs(this Vector3 vector)
		{
			return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
		}

		public static Vector2 Abs(this Vector2 vector)
		{
			return new Vector2(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
		}

		public static Vector2 RandomInRange(this Vector2 vector, float range)
		{
			return new Vector2(vector.x + Random.Range(-range, range), vector.y + Random.Range(-range, range));
		}
		
		public static Vector2 RandomInRange(this Vector2 vector, float xRange, float yRange)
		{
			return new Vector2(vector.x + Random.Range(-xRange, xRange), vector.y + Random.Range(-yRange, yRange));
		}
		
		public static Vector3 RandomInRange(this Vector3 vector, float xRange, float yRange, float zRange)
		{
			return new Vector3(vector.x + Random.Range(-xRange, xRange), vector.y + Random.Range(-yRange, yRange), vector.z + Random.Range(-zRange, zRange));
		}	
		
		public static Vector3 RandomInRange(this Vector3 vector, float xRange, float yRange)
		{
			return new Vector3(vector.x + Random.Range(-xRange, xRange), vector.y + Random.Range(-yRange, yRange), vector.z);
		}
	}
}