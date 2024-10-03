#if UNITY_IOS || UNITY_TVOS

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using System.IO;

namespace BerserkV3.Editor.IOS
{
	public static class PodFilePostProcess
	{
		[PostProcessBuild(45)] //must be between 40 and 50 to ensure that it's not overriden by Podfile generation (40) and that it's added before "pod install" (50)
		private static void PostProcessBuild_iOS(BuildTarget target, string buildPath)
		{
			if (target != BuildTarget.iOS)
				return;

			Debug.Log(
				$"{nameof(PodFilePostProcess)} Start Post Process Build");

			var content = "\n\npost_install do |installer|\n" +
			              "installer.pods_project.targets.each do |target|\n" +
			              "  target.build_configurations.each do |config|\n" +
			              $"    config.build_settings['IPHONEOS_DEPLOYMENT_TARGET'] = '{PlayerSettings.iOS.targetOSVersionString}'\n" +
			              "     config.build_settings['ENABLE_BITCODE'] = 'NO'\n" +
			              "  end\n" +
			              " end\n" +
			              "end\n";

			using var streamWriter = File.AppendText(Path.Combine(buildPath, "Podfile"));
			streamWriter.WriteLine(content);

			Debug.Log(
				$"{nameof(PodFilePostProcess)} End success Post Process Build");
		}
	}
}
#endif