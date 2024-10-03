using System;

namespace RR.Core.ResourceManagament
{
	public class NotInitializedException : Exception
	{
		public NotInitializedException() : base("Before using the service, it must be initialized.") {}
	}
}