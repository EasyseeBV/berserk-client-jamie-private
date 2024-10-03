using RR.Core.DebugSystem;
using RR.Core.Extensions;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RR.Core.Editor
{
#if RR_ASSETBUS
	public class AllAssetPostprocessor : AssetPostprocessor
	{
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			if (importedAssets != null && importedAssets.Any())
			{
				//RRLogger.Log("Added assets:".Lightblue() + string.Join("\n", importedAssets.Select(Path.GetFileName).ToList()));
				AssetProcessorBus.AddedAssets += AssetUtility.LoadAssetsAtAbsolutePath<MonoBehaviour>(importedAssets).ToList();
			}
		}
	}
#endif
}