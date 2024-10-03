using System;

namespace BerserkV3.Common.Utils
{
	public class Disposable : IDisposable
	{
		protected bool IsDisposed { get; private set; }
		
		public virtual void Dispose()
		{
			if (IsDisposed)
				throw new ObjectDisposedException(GetType().FullName);

			IsDisposed = true;
		}
	}
}