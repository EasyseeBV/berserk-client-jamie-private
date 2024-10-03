namespace BerserkV3.Common.PreviewSystem
{
	public struct PreviewSettings : IPreviewSetting
	{
		public PreviewType PreviewType { get; }
		public float PreviewTime { get; }
		public bool Enabled { get; set; }

		public PreviewSettings(PreviewType type, float previewTime = -1, bool enabled = true)
		{
			PreviewType = type;
			PreviewTime = previewTime;
			Enabled = enabled;
		}
	}
}