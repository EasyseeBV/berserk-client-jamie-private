using System;

namespace RR.Network.CacheSystem
{
	public class InvalidCacheTypeException : Exception
	{
		public InvalidCacheTypeException(string msg) : base(msg)
		{
		}
	}
}
