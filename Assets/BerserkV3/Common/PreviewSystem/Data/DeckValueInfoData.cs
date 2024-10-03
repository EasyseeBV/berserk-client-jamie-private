using System;

namespace BerserkV3.Common.PreviewSystem
{
	public class DeckValueInfoData : IPreviewData
	{
		public string Title { get; set; }
		public string Description { get; set; }

		public event Action OnUpdatePreview;
		
		public void Dispose()
		{
			OnUpdatePreview = null;
		}
	}
}