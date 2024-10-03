namespace RR.Core.Extensions
{
	public static class NumericExtensions
	{
		public static bool IsBetween(this int val, int min, int max) 
			=> val >= min && val <= max;
		
		public static bool IsBetween(this float val, float min, float max) 
			=> val >= min && val <= max;
		
		public static bool IsBetween(this byte val, int min, int max) 
			=> val >= min && val <= max;
	}
}