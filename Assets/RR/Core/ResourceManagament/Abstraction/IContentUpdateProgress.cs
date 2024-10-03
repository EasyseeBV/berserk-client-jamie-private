using System;

namespace RR.Core.ResourceManagament
{
	public interface IContentUpdateProgress : IProgress<float>
	{
		void OnSkip();
		void OnStarted();
		void OnEnded();
	}
}