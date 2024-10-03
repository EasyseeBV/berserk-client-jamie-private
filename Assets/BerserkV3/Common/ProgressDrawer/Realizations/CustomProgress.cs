using System;
using UnityEngine;

namespace BerserkV3.Common.ProgressDrawer
{
	public class CustomProgress : IProgress<float>, IDisposable
	{
		public Progress<float> Other;
		public float Progress { get; private set; }
		public event Action<CustomProgress> OnProgressChanged;

		public bool Disposed { get; private set; }

		public CustomProgress(Progress<float> other = null)
		{
			if (other == null)
				return;

			Other = other;
			Other.ProgressChanged += OnOtherProgressChanged;
		}

		public void Report(float progress01)
		{
			if (Disposed)
				return;
			
			if (progress01 == 0)
				progress01 = 0.001f; // minimum report of progress
					
			Progress = Mathf.Clamp01(progress01);
			OnProgressChanged?.Invoke(this);
		}

		public void Dispose()
		{
			if (Disposed)
				return;
			
			Disposed = true;
			OnProgressChanged?.Invoke(this);
			OnProgressChanged = null;
			if (Other is IDisposable disposable)
				disposable.Dispose();
			
			if (Other != null)
				Other.ProgressChanged -= OnOtherProgressChanged;

			Other = null;
		}
		
		private void OnOtherProgressChanged(object sender, float value)
		{
			Report(value);
		}
	}
}