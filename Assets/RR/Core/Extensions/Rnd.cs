using UnityEngine;

namespace RR.Core.Extensions
{
	public static class Rnd
	{
		public static int MinusOneOrPlusOne() => Random.Range(0, 2) * 2 - 1;
		public static Color ColorNoAlpha() => Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
	}
}