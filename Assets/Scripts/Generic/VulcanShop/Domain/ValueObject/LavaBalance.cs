using System;

namespace Vulcan.Shop.Domain
{
	public class LavaBalance
	{
		public double Value { get; } = default;

		public LavaBalance(double value)
		{
			if (value < 0)
				new ArgumentOutOfRangeException();

			Value = value;
		}
	}
}