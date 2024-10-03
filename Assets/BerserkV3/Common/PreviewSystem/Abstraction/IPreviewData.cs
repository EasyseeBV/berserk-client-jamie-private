using System;

namespace BerserkV3.Common.PreviewSystem
{
	public interface IPreviewData : IDisposable
	{
		event Action OnUpdatePreview;
	}
}