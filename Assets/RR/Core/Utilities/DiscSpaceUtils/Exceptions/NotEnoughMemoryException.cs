using System;

namespace RR.Core.Utilities.DiscSpaceUtils
{
	public class NotEnoughMemoryException : Exception
	{
		public NotEnoughMemoryException(long bytes) : base(
			$"There is not enough free space on the device, free up " +
			$"{bytes.FormatBytes()} of memory and try again.") {}
	}
}