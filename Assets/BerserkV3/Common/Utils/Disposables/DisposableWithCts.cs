using System.Threading;

namespace BerserkV3.Common.Utils
{
	public class DisposableWithCts : Disposable
	{
		protected readonly CancellationTokenSource Cts;
		protected CancellationToken Token => Cts?.Token ?? CancellationToken.None;

		protected DisposableWithCts()
		{
			Cts = new CancellationTokenSource();
		}

		public override void Dispose()
		{
			base.Dispose();
			
			Cts.Cancel();
			Cts.Dispose();
		}
	}
}