namespace BerserkV3.Common.PreviewSystem
{

	public class PreviewSystemAdapter
	{
		public static IPreviewSystem Instance { get; private set; }
		
		public PreviewSystemAdapter(IPreviewSystem previewSystem)
		{
			Instance = previewSystem;
		}
	}

}