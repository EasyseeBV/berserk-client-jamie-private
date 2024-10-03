using RR.Core.DebugSystem;
using RR.Core.Extensions;

namespace RR.Core.Utilities.DiscSpaceUtils
{
	public static class DiscSpaceChecker
	{
#if !UNITY_EDITOR && UNITY_IOS
		[System.Runtime.InteropServices.DllImport("__Internal")]
		private static extern long getAvailableDiskSpace();
		
		public static long GetFreeDiskBytes(string path)
		{
			long bytes = 0;
			try
			{
				bytes = getAvailableDiskSpace();
				LogFreeMemory(bytes, path);
			}
			catch (System.Exception e)
			{
				LogFreeMemory(bytes, $"Exception : {e.Message}, path : {path}");
			}
			return bytes;
		}
#elif !UNITY_EDITOR && UNITY_ANDROID  // minimum API 18 - Android 4.3

		public static long GetFreeDiskBytes(string path)
		{
			var statFs = new UnityEngine.AndroidJavaObject("android.os.StatFs", path);
			var freeMemoryBytes = System.Math.Abs(statFs.Call<long>("getAvailableBytes"));
			LogFreeMemory(freeMemoryBytes, path);
			return freeMemoryBytes;
		}
#else // Windows, Mac, Linux
		public static long GetFreeDiskBytes(string path)
		{
			// Get the drive where the game is running
			var drive = new System.IO.DriveInfo(System.IO.Path.GetPathRoot(path));

			// Check if the drive is ready
			if (drive.IsReady)
			{
				var freeMemoryBytes = System.Math.Abs(drive.AvailableFreeSpace);
				LogFreeMemory(freeMemoryBytes, path);
				return freeMemoryBytes;
			}

			throw new System.Exception($"Drive is not Ready : {drive}");
		}
#endif
		public static void ThrowWhenNotEnoughMemoryFor(long needBytesSize, string path = "")
		{
			if (needBytesSize > GetFreeDiskBytes(path))
				throw new NotEnoughMemoryException(needBytesSize);
		}
		
		public static string FormatBytes(this long bytes)
		{
			string[] suffix = { "B", "KB", "MB", "GB", "TB" };
			int i;
			double dblSByte = bytes;
			for (i = 0; i < suffix.Length && bytes >= 1024; i++, bytes /= 1024) 
			{
				dblSByte = bytes / 1024.0;
			}

			return $"{dblSByte:0.##} {suffix[i]}";
		}
		
		private static void LogFreeMemory(long bytes, string path)
		{
			RRLogger.Log($"[{nameof(DiscSpaceChecker).Orange()}] " +
			             $"Free memory space on disk : {bytes.FormatBytes()}\n" +
			             $"Platform : {UnityEngine.Application.platform.ToString()}\n" +
			             $"Path : {path}");
		}
	}
}