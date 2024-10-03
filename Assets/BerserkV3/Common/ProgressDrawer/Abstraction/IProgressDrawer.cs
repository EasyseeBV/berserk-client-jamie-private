using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Common.ProgressDrawer
{
	public enum ProgressType
	{
		Default = 0,
		Versus,
	}
	
	public interface IProgressDrawer
	{
		UniTask SetProgressTypeAsync(ProgressType value);
		void AddProgress(Progress<float> other = null);
		IProgress<float> CreateProgress(Progress<float> other = null);
		IEnumerable<IProgress<float>> GetExistProgress();
		void CancelProgress(IProgress<float> value);
		void SetTooltip(string value);
		void SetProgressFormat(string value);
		UniTask ShowAsync(params object[] args);
		UniTask CloseAsync(bool force = false, params object[] args);
		void Reset();
	}
}