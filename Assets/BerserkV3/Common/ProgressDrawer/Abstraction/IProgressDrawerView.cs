using System.Threading;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Common.ProgressDrawer
{
	public interface IProgressDrawerView
	{
		IProgressDawerBarLayout ProgressBarLayout { get; }

		void SetProgressType(ProgressType value);
		
		UniTask ShowAsync(CancellationToken token = default, params object[] args);
		
		UniTask CloseAsync(bool force = false, CancellationToken token = default, params object[] args);
	}
}