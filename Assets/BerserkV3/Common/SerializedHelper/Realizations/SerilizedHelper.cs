using System.IO;

namespace BerserkV3.Common.SerializedHelper
{
	public static class SerilizedHelper
	{
		public const string FILE_NAME = "PreviousLogin.berserk";
		public static string Path => GetPath();

#if UNITY_EDITOR
		[UnityEditor.MenuItem( "Tools/Clear Serialized Data")]
#endif
		public static void Clear()
		{
			if(File.Exists(Path))
				File.Delete(Path);
		}

		public static string GetPath()
		{
#if UNITY_EDITOR
			return System.IO.Path.Combine("Library/", FILE_NAME);
#elif UNITY_STANDALONE
			return System.IO.Path.Combine(UnityEngine.Application.dataPath, FILE_NAME);
#else
			RR.Core.Utilities.RRFile.EnsureSaveDataDirectoryExists();
			return System.IO.Path.Combine(RR.Core.Utilities.RRFile.SaveDataDir, FILE_NAME);
#endif
		}
	}
}