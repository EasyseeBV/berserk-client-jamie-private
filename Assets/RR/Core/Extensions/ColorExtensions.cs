using UnityEngine;

namespace RR.Core.Extensions
{
	public static class ColorExtensions
	{
		public static Color SetAlpha(this Color color, float alpha) => new Color(color.r, color.g, color.b, alpha);
		public static Color SetRGB(this Color color, float r, float g, float b) => new Color(r, g, b, color.a);
		public static Color SetR(this Color color, float r) => color.SetRGB(r, color.g, color.b);
		public static Color SetG(this Color color, float g) => color.SetRGB(color.r, g, color.b);
		public static Color SetB(this Color color, float b) => color.SetRGB(color.r, color.g, b);
	}
}