using UnityEngine;

namespace BerserkV3.Common.PreviewSystem
{
	public interface IPreviewable
	{
		/// <summary>
		/// Target of some reference view
		/// </summary>
		GameObject TargetView { get; }
		
		IPreviewData PreviewData { get; }
		
		IPreviewSetting PreviewSettings { get; }
		
		bool CanPreview();
	}
}