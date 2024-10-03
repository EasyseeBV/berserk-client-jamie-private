using RR.Core.DebugSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;


#if UNITY_IOS
using System.IO;
using UnityEditor.iOS.Xcode;
#endif

namespace RR.Core.Editor.Tools
{
	/// <inheritdoc />
	/// <summary>
	/// The methods in this class are called from Unity Cloud Build
	/// </summary>
	public class PlayerBuild : MonoBehaviour
    {
        private const string CACHED_SCENE_PREFIX = "original-";
        private const string DEVELOPMENT_MENU = EditorUtils.ComName + "/Build/Development/";
        private const string PRODUCTION_MENU = EditorUtils.ComName + "/Build/Production/";
        private static string originalScene;

        private static readonly List<string> scenesBeforePlatformChanges = new List<string>();

#region Menu Functions

#region Menu Items

		[MenuItem(DEVELOPMENT_MENU + "Windows 64 bit", priority = 50)]
        public static void BuildWin64Development() => BuildFromMenu(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, "exe", true);

        [MenuItem(DEVELOPMENT_MENU + "Mac OSX", priority = 51)]
        public static void BuildMacOSXDevelopment() => BuildFromMenu(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX, "app", true);

        [MenuItem(DEVELOPMENT_MENU + "Android", priority = 53)]
        public static void BuildAndroidDevelopment() => BuildFromMenu(BuildTargetGroup.Android, BuildTarget.Android, "apk", true);

        [MenuItem(DEVELOPMENT_MENU + "iOS", priority = 54)]
        public static void BuildIOSDevelopment() => BuildFromMenu(BuildTargetGroup.iOS, BuildTarget.iOS, "/", true);

        [MenuItem(PRODUCTION_MENU + "Windows 64 bit", priority = 50)]
        public static void BuildWin64() => BuildFromMenu(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, "exe", false);

        [MenuItem(PRODUCTION_MENU + "Mac OSX", priority = 51)]
        public static void BuildMacOSX() => BuildFromMenu(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX, "app", false);

        [MenuItem(PRODUCTION_MENU + "Android", priority = 53)]
        public static void BuildAndroid() => BuildFromMenu(BuildTargetGroup.Android, BuildTarget.Android, "apk", false);

        [MenuItem(PRODUCTION_MENU + "iOS", priority = 54)]
        public static void BuildIOS() => BuildFromMenu(BuildTargetGroup.iOS, BuildTarget.iOS, "/", false);

        [MenuItem(PRODUCTION_MENU + "Linux", priority = 55)]
        public static void BuildLinux() => BuildFromMenu(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64, "/", false);

#endregion

        private static void BuildFromMenu(BuildTargetGroup targetGroup, BuildTarget target, string extension, bool isDevelopment)
        {
            // for folders, open a dialog to select a folder to build to. For files, choose a filename to save to.
            var buildPath = extension == "/"
                ? EditorUtility.OpenFolderPanel("Build Destination", string.Empty, string.Empty)
                : EditorUtility.SaveFilePanel("Build Destination", string.Empty, Application.productName, extension);

            // if no path was selected
            if (string.IsNullOrEmpty(buildPath))
            {
                RRLogger.Log("No path selected to build to. Cancelling build.");
                return;
            }

            PreBuildProcesses(targetGroup, target);
            RRLogger.Log("Done pre build processes. Building player, is development build : " + isDevelopment);

            // set the build options based on if this is a development build or not
            var options = isDevelopment
                ? BuildOptions.Development | BuildOptions.ConnectWithProfiler
                : BuildOptions.None;

            Build(target, buildPath, options);
            RRLogger.Log("Done building player. Starting post build processes...");
            PostBuildProcesses();
        }

#endregion

#region Cloud Build Functions

        /*
 			ref: https://docs.unity3d.com/Manual/UnityCloudBuildPreAndPostExportMethods.html
 		 */
        public static void CloudBuildPreExport()
        {
#if UNITY_IOS
 			PreBuildProcesses(BuildTargetGroup.iOS, BuildTarget.iOS);
#elif UNITY_ANDROID
 			PreBuildProcesses(BuildTargetGroup.Android, BuildTarget.Android);
#elif UNITY_STANDALONE_WIN
 			PreBuildProcesses(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
#elif UNITY_STANDALONE_OSX
 			PreBuildProcesses(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);
#elif UNITY_STANDALONE_LINUX
            PreBuildProcesses(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64);
#else
 			RRLogger.Error("Error with PreExport Cloud build. Unsupported platform");
#endif
        }

        /*
 			release only
 		 */
        public static void CloudBuildPostExportIOSRelease(string xcodeExportPath)
        {
#if UNITY_IOS
            // Get plist
            var plistPath = xcodeExportPath + "/Info.plist";
            UnityEditor.iOS.Xcode.PlistDocument plist = new UnityEditor.iOS.Xcode.PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));
            RRLogger.Log("PLIST: Got plist document");
            // Get root
            UnityEditor.iOS.Xcode.PlistElementDict rootDict = plist.root;

            // Change value of CFBundleVersion and CFBundleShortVersionString in Xcode plist
            var version = Application.version;

            if (!string.IsNullOrEmpty(version))
            {
                rootDict.SetString("CFBundleVersion", version);
                rootDict.SetString("CFBundleShortVersionString", version);
                RRLogger.Log("PLIST: set plist version keys: " + version);
            }

            // change this setting as well
            const string encryptionKey = "ITSAppUsesNonExemptEncryption";
            rootDict.SetBoolean(encryptionKey, false);

            RRLogger.Log("PLIST: set AppUsesNonExemptEncryption to false");

            // Write to file
            File.WriteAllText(plistPath, plist.WriteToString());
            RRLogger.Log("PLIST: wrote plist changes to file");
#endif
        }

        /*
 			Note: "If you’ve tagged any methods in your code with the Unity PostProcessBuildAttribute,
 			those methods are executed before any methods configured as post-export methods in Unity Cloud Build."
 			ref: https://docs.unity3d.com/Manual/UnityCloudBuildPreAndPostExportMethods.html
 		 */
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
        {
#if UNITY_IOS
 			RRLogger.Log("RJGPlayerBuild.OnPostprocessBuild(), inside `#if UNITY_IOS` section");
 			if (buildTarget == BuildTarget.iOS)
 			{
 				RRLogger.Log("[UCB Demos] OnPostprocessBuildiOS");
 				string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";

 				PBXProject proj = new PBXProject();
 				proj.ReadFromString(File.ReadAllText(projPath));

#if UNITY_2019_3_OR_NEWER                
 				string target = proj.GetUnityMainTargetGuid();
#else
 				string target = proj.TargetGuidByName("Unity-iPhone");
#endif
 				// Set a custom link flag
 				proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC");
 				proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-lz");

 				File.WriteAllText(projPath, proj.WriteToString());
 			}
#endif
        }

#endregion

        private static void PreBuildProcesses(BuildTargetGroup targetGroup, BuildTarget target)
        {
            // Fix for error after adding Firebase to project
            if (targetGroup == BuildTargetGroup.Android || target == BuildTarget.Android)
                EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

            BuildAddressables();
            Caching.ClearCache();
            ApplyPlatformChangesToEveryScene(targetGroup, target);
        }

        private static void BuildAddressables()
        {
	       // UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.CleanPlayerContent();
	       // UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.BuildPlayerContent();
        }

        private static void ApplyPlatformChangesToEveryScene(BuildTargetGroup targetGroup, BuildTarget target)
        {
            // remember what scene was open in the editor before we start
            originalScene = SceneManager.GetActiveScene().path;

            // change the current platform of the editor
            EditorUserBuildSettings.selectedBuildTargetGroup = targetGroup;
            var successfullySwitched = EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup, target);

            RRLogger.Log("Active build target is: " + EditorUserBuildSettings.activeBuildTarget);

            if (!successfullySwitched)
            {
	            RRLogger.Log("Failed to switch platform to " + targetGroup);
				return;
            }

            // for every scene to be included in the build
            foreach (var scene in EditorBuildSettings.scenes)
            {
	            // skip this scene if it is not to be included in the build
	            if (!scene.enabled)
		            continue;

	            // load the scene in the editor
	            var path = scene.path;
	            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
	            RRLogger.Log("loaded scene " + path);

	            // before changing the scene, make a copy of it and cache it, so it can be restored after building.
	            var cachedPath = PrefixAssetName(path, CACHED_SCENE_PREFIX);
	            if (AssetDatabase.CopyAsset(path, cachedPath))
		            scenesBeforePlatformChanges.Add(cachedPath);

	            // save the scene
	            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), path);
            }
        }

        private static void PostBuildProcesses()
        {
            RevertScenes();
        }

        private static void RevertScenes()
        {
            // Call after the scenes have been modified by ApplyPlatformChangesToEveryScene().
            // Any scene files that were modified, will have had a copy made before modification and saved, with a prefix preprended to the name.
            // Go through scene that was saved before modification, and overwrite the modified version of that scene with the original.
            foreach (var path in scenesBeforePlatformChanges)
            {
                var originalPath = path.Replace(CACHED_SCENE_PREFIX, "");
                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                RRLogger.Log("resetting scene copy, paths are " + path + " and " + originalPath);

                // open the copy of the original version of the scene
                var openScene = SceneManager.GetActiveScene();

                // overwrite the modified version of the scene with it
                EditorSceneManager.SaveScene(openScene, originalPath);
                EditorSceneManager.OpenScene(originalPath);
            }

            // after reverting all the scenes, delete the cached versions of them.
            foreach (var path in scenesBeforePlatformChanges)
            {
                var success = AssetDatabase.DeleteAsset(path);
                RRLogger.Log("successfully deleted " + path + "? : " + success);
            }

            // reset the list of cached scene paths.
            scenesBeforePlatformChanges.Clear();

            // return to the original scene
            if (!string.IsNullOrEmpty(originalScene))
                EditorSceneManager.OpenScene(originalScene);
        }

        private static void Build(BuildTarget target, string path, BuildOptions options)
        {
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = GetScenesArray(), locationPathName = path, target = target, options = options
            };
            // set builds to be development builds and auto connect to profiler by default.
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            if (report.summary.totalErrors > 0)
                RRLogger.Error("Build Error: " + report.summary.outputPath);
        }

#region Helper Methods

        private static string[] GetScenesArray()
        {
            var ebsScenes = EditorBuildSettings.scenes;
            return (from scene in ebsScenes where scene.enabled select scene.path).ToArray();
        }

        private static string PrefixAssetName(string assetPath, string prefix)
        {
            // inserts the prefix into the string assetPath immediately after the last forward slash.
            var lastSlashIndex = assetPath.LastIndexOf("/", StringComparison.Ordinal);
            return assetPath.Substring(0, lastSlashIndex + 1) + prefix + assetPath.Substring(lastSlashIndex + 1);
        }

#endregion
    }
}
