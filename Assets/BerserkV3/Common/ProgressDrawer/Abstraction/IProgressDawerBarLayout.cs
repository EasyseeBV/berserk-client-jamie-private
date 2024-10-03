using System.Threading;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Common.ProgressDrawer
{
	public interface IProgressDawerBarLayout
	{
		void SetActiveTooltip(bool value);

		void SetTooltipText(string value);

		void SetProgressFormat(string value);

		void SetProgress(float value);
		
		UniTask ShowAsync(CancellationToken token = default);

		UniTask CloseAsync(bool force = false, CancellationToken token = default);
	}
}