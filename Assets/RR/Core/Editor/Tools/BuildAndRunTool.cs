using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RR.Core.Editor.Tools
{
    // taken from https://www.appsfresh.com/blog/multiplayer/ and rewritten
	[Obsolete(nameof(PlayerBuild))]
    internal static class BuildAndRunTool
    {
        private static string GetProjectName()
        {
            var s = Application.dataPath.Split('/');
            return s[s.Length - 2];
        }

        private static string[] GetScenePaths()
        {
            var scenes = new string[EditorBuildSettings.scenes.Length];

            for (var i = 0; i < scenes.Length; i++) scenes[i] = EditorBuildSettings.scenes[i].path;

            return scenes;
        }

        private static void MakeAndSpawnInstances(int count, BuildTarget targetPlatform, bool dev = true)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, targetPlatform);
            var buildPath = $"Build/{targetPlatform.ToString()}/{GetProjectName()}.exe";
            BuildPipeline.BuildPlayer(
                GetScenePaths(),
                buildPath,
                targetPlatform,
                dev ? BuildOptions.Development : BuildOptions.None
            );

            for (var i = 1; i <= count; i++)
            {
                var process = new Process
                {
                    StartInfo =
                    {
                        FileName = Path.Combine(Application.dataPath, "..", buildPath)
                    }
                };
                process.Start();
            }
        }

#if UNITY_EDITOR_WIN
        [MenuItem(EditorUtils.ComName + "/Multiwindow/Windows/Run 1 Player", false)]
        private static void PerformWin64Build1()
        {
            MakeAndSpawnInstances(1, BuildTarget.StandaloneWindows64);
        }

        [MenuItem(EditorUtils.ComName + "/Multiwindow/Windows/Run 2 Players", false)]
        private static void PerformWin64Build2()
        {
            MakeAndSpawnInstances(2, BuildTarget.StandaloneWindows64);
        }

        [MenuItem(EditorUtils.ComName + "/Multiwindow/Windows/Run 3 Players", false)]
        private static void PerformWin64Build3()
        {
            MakeAndSpawnInstances(3, BuildTarget.StandaloneWindows64);
        }

        [MenuItem(EditorUtils.ComName + "/Multiwindow/Windows/Run 4 Players", false)]
        private static void PerformWin64Build4()
        {
            MakeAndSpawnInstances(4, BuildTarget.StandaloneWindows64);
        }

        [MenuItem(EditorUtils.ComName + "/Multiwindow/Windows/Run 5 Players", false)]
        private static void PerformWin64Build5()
        {
            MakeAndSpawnInstances(5, BuildTarget.StandaloneWindows64);
        }

        [MenuItem(EditorUtils.ComName + "/Multiwindow/Windows/Run 6 Players", false)]
        private static void PerformWin64Build6()
        {
            MakeAndSpawnInstances(6, BuildTarget.StandaloneWindows64);
        }

        [MenuItem(EditorUtils.ComName + "/Multiwindow/Windows/Run 7 Players", false)]
        private static void PerformWin64Build7()
        {
            MakeAndSpawnInstances(7, BuildTarget.StandaloneWindows64);
        }
#endif

#if UNITY_EDITOR_LINUX
        [MenuItem(EditorUtils.ComName + "/Multiwindow/Linux/Run 1 Player", false)]
        private static void PerformLinuxBuild1()
        {
            MakeAndSpawnInstances(2, BuildTarget.StandaloneLinux64);
        }

        [MenuItem(EditorUtils.ComName + "/Multiwindow/Linux/Run 2 Players", false)]
        private static void PerformLinuxBuild2()
        {
            MakeAndSpawnInstances(2, BuildTarget.StandaloneLinux64);
        }

        [MenuItem(EditorUtils.ComName + "/Multiwindow/Linux/Run 3 Players", false)]
        private static void PerformLinuxBuild3()
        {
            MakeAndSpawnInstances(3, BuildTarget.StandaloneLinux64);
        }

        [MenuItem(EditorUtils.ComName + "/Multiwindow/Linux/Run 4 Players", false)]
        private static void PerformLinuxBuild4()
        {
            MakeAndSpawnInstances(4, BuildTarget.StandaloneLinux64);
        }

        [MenuItem(EditorUtils.ComName + "/Multiwindow/Linux/Run 5 Players", false)]
        private static void PerformLinuxBuild5()
        {
            MakeAndSpawnInstances(5, BuildTarget.StandaloneLinux64);
        }

        [MenuItem(EditorUtils.ComName + "/Multiwindow/Linux/Run 6 Players", false)]
        private static void PerformLinuxBuild6()
        {
            MakeAndSpawnInstances(6, BuildTarget.StandaloneLinux64);
        }

        [MenuItem(EditorUtils.ComName + "/Multiwindow/Linux/Run 7 Players", false)]
        private static void PerformLinuxBuild7()
        {
            MakeAndSpawnInstances(7, BuildTarget.StandaloneLinux64);
        }
#endif

#if UNITY_EDITOR_OSX
        [MenuItem(EditorUtils.ComName + "/Multiwindow/MacOS/Run 1 Player", false)]
        private static void PerformMacOSBuild1()
        {
            MakeAndSpawnInstances(1, BuildTarget.StandaloneOSX);
        }

        [MenuItem(EditorUtils.ComName + "/Multiwindow/MacOS/Run 2 Players", false)]
        private static void PerformMacOSBuild2()
        {
            MakeAndSpawnInstances(2, BuildTarget.StandaloneOSX);
        }

        [MenuItem(EditorUtils.ComName + "/Multiwindow/MacOS/Run 3 Players", false)]
        private static void PerformMacOSBuild3()
        {
            MakeAndSpawnInstances(3, BuildTarget.StandaloneOSX);
        }

        [MenuItem(EditorUtils.ComName + "/Multiwindow/MacOS/Run 4 Players", false)]
        private static void PerformMacOSBuild4()
        {
            MakeAndSpawnInstances(4, BuildTarget.StandaloneOSX);
        }

        [MenuItem(EditorUtils.ComName + "/Multiwindow/MacOS/Run 5 Players", false)]
        private static void PerformMacOSBuild5()
        {
            MakeAndSpawnInstances(5, BuildTarget.StandaloneOSX);
        }

        [MenuItem(EditorUtils.ComName + "/Multiwindow/MacOS/Run 6 Players", false)]
        private static void PerformMacOSBuild6()
        {
            MakeAndSpawnInstances(6, BuildTarget.StandaloneOSX);
        }

        [MenuItem(EditorUtils.ComName + "/Multiwindow/MacOS/Run 7 Players", false)]
        private static void PerformMacOSBuild7()
        {
            MakeAndSpawnInstances(7, BuildTarget.StandaloneOSX);
        }
#endif
    }
}
