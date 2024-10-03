namespace BerserkV3.Common.PreviewSystem
{
	public interface IPreviewSetting
	{
		PreviewType PreviewType { get; }
		
		float PreviewTime { get; }
		
		bool Enabled { get; set; }
	}
}