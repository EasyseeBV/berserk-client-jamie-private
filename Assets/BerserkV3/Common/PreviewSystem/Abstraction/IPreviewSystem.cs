using System;

namespace BerserkV3.Common.PreviewSystem
{
	public interface IPreviewSystem
	{
		IPreviewable Current { get; }
		
		event Action OnPreview;
		
		event Action OnClose;
		
		void Registration(IPreviewable previewable);
		
		void UnRegistration(IPreviewable previewable);
		
		void Preview(IPreviewable previewable);

		void Lock(bool value);
		
		void Close();

		void Clear();
	}
}