using System.IO;
using System.Linq;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using UnityEditor;
using UnityEngine;

namespace RR.Core.Editor.Tools
{
	public class AssetBundleBuilder
	{
		private static string path => Application.streamingAssetsPath;

		[MenuItem(EditorUtils.ComName + "/Bundles/Build " + nameof(BuildTarget.Android))]
		public static void BuildAllAssetBundles_Android() => BuildAllAssetBundles(BuildTarget.Android);

		[MenuItem(EditorUtils.ComName + "/Bundles/Build " + nameof(BuildTarget.iOS))]
		public static void BuildAllAssetBundles_iOS() => BuildAllAssetBundles(BuildTarget.iOS);

		[MenuItem(EditorUtils.ComName + "/Bundles/Build " + nameof(BuildTarget.StandaloneWindows64))]
		public static void BuildAllAssetBundles_StandaloneWindows64() => BuildAllAssetBundles(BuildTarget.StandaloneWindows64);

		[MenuItem(EditorUtils.ComName + "/Bundles/" + nameof(BuildAssetBundleOptions.ForceRebuildAssetBundle) + "_" + nameof(BuildTarget.Android), priority = 50)]
		public static void ForceBuildAllAssetBundles_Android() => BuildAllAssetBundles(BuildTarget.Android, BuildAssetBundleOptions.ForceRebuildAssetBundle);

		[MenuItem(EditorUtils.ComName + "/Bundles/" + nameof(BuildAssetBundleOptions.ForceRebuildAssetBundle) + "_" + nameof(BuildTarget.iOS), priority = 50)]
		public static void ForceBuildAllAssetBundles_iOS() => BuildAllAssetBundles(BuildTarget.iOS, BuildAssetBundleOptions.ForceRebuildAssetBundle);

		[MenuItem(EditorUtils.ComName + "/Bundles/" + nameof(BuildAssetBundleOptions.ForceRebuildAssetBundle) + "_" + nameof(BuildTarget.StandaloneWindows64), priority = 50)]
		public static void ForceBuildAllAssetBundles_StandaloneWindows64() => BuildAllAssetBundles(BuildTarget.StandaloneWindows64, BuildAssetBundleOptions.ForceRebuildAssetBundle);

		private static void BuildAllAssetBundles(BuildTarget target, BuildAssetBundleOptions options = BuildAssetBundleOptions.None)
		{
			var finalPath = path + $"/{target}";
			RRLogger.Log($"Creating assets in: {finalPath}");

			if (!Directory.Exists(finalPath))
				Directory.CreateDirectory(finalPath);

			var result = BuildPipeline.BuildAssetBundles(finalPath, options, target);
			RRLogger.Log($"Successfully created {result.GetAllAssetBundles().Length.ToString().Bold().Green()} bundle items");
		}

		[MenuItem(EditorUtils.ComName + "/Bundles/DeleteLocalBundles", priority = 90)]
		public static void DeleteLocalBundles()
		{
			new DirectoryInfo(path)
				.GetFiles()
				.Where(f => !f.Name.Contains("."))
				.ForEach(f => File.Delete(Path.Combine(path, f.Name)));

			Debug.Log("All Bundles were deleted");
		}
	}
}
