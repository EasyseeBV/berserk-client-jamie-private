namespace BerserkV3.Editor.ArtUploader
{
	public class UploadFileModel
	{
		public FormFile File { get; set; }
		public string Category { get; set; }
		public string DataType { get; set; }
		public string ArtKey { get; set; }

		public class FormFile
		{
			public string FileName { get; set; }
			public string ContentType { get; set; }
			public byte[] Data { get; set; }
		}
	}
}