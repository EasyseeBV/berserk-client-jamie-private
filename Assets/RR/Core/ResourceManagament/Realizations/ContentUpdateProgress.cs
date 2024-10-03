using System;

namespace RR.Core.ResourceManagament
{
	public class ContentUpdateProgress : Progress<float>, IContentUpdateProgress, IDisposable
	{
		public event Action<float> OnUpdate;
		public event Action OnStart;
		public event Action OnEnd;

		protected override void OnReport(float value)
		{
			base.OnReport(value);
			OnUpdate?.Invoke(value);
		}

		public void OnSkip()
		{
			OnReport(1);
		}

		public void OnStarted()
		{
			OnStart?.Invoke();
		}
		
		public void OnEnded()
		{
			OnEnd?.Invoke();
		}

		public void Dispose()
		{
			OnUpdate = null;
			OnStart = null;
			OnEnd = null;
		}
	}
}