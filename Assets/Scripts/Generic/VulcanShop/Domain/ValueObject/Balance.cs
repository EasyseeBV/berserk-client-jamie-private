using System;

namespace Vulcan.Shop.Domain
{
	public class Balance
	{
		public double Value { get; }

		public Balance(double value)
		{
			if (value < 0)
				new ArgumentOutOfRangeException();

			Value = value;
		}
	}
}