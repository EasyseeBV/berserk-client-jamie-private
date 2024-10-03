#if UNITY_IOS || UNITY_TVOS

using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace BerserkV3.Editor.IOS
{
	public static class ModifyFrameworksPostProcess
	{
		private const string SWIFT_STANDARD_LIBRARIES = "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES";
		private const string ALWAYS_SEARCH_USER_PATHS = "ALWAYS_SEARCH_USER_PATHS";
		private const string NOTIFICATIONSERVICE = "notificationservice";
		private const string USE_HEADERMAP = "USE_HEADERMAP";
		private const string BIT_CODE = "ENABLE_BITCODE";
		private const string NO = "NO";
		private const string YES = "YES";

		[PostProcessBuild(999)]
		public static void OnPostProcessBuild(BuildTarget target, string path)
		{
			if (target != BuildTarget.iOS)
				return;

			try
			{
				Debug.Log($"{nameof(ModifyFrameworksPostProcess)} Start Post Process Build");
				var projPath = PBXProject.GetPBXProjectPath(path);
				Debug.Log($"{nameof(projPath)}: {projPath}");

				var project = new PBXProject();
				project.ReadFromFile(projPath);

				Debug.Log($"{nameof(ModifyFrameworksPostProcess)} Project successfully initialized");

				project.DisableSwiftStandardLibraries();
				project.EnableSwiftStandardLibraries();
				project.DisableBitcode();
				project.WriteToFile(projPath);
				Debug.Log($"{nameof(ModifyFrameworksPostProcess)} End success Post Process Build");
			}
			catch (Exception e)
			{
				Debug.LogError(
					$"{nameof(ModifyFrameworksPostProcess)} End failed Post Process Build. Message: {e.Message}");
				Debug.LogException(e);
				throw;
			}
		}

		private static void AddImagesXcAssetsToBuildPhases(this PBXProject project)
		{
			var mainGuid = project.GetUnityMainTargetGuid();
			project.AddFileToBuild(mainGuid, project.AddFile("Unity-iPhone/Images.xcassets", "Images.xcassets"));
			Debug.Log($"{nameof(ModifyFrameworksPostProcess)} {nameof(AddImagesXcAssetsToBuildPhases)} end");
		}

		private static void EnableSwiftStandardLibraries(this PBXProject project)
		{
			var targetIds = new[]
			{
				project.GetUnityMainTargetGuid()
			};

			foreach (var targetId in targetIds)
				project.SafeSetBuildProperty(targetId, SWIFT_STANDARD_LIBRARIES, YES);

			Debug.Log($"{nameof(ModifyFrameworksPostProcess)} {nameof(EnableSwiftStandardLibraries)} end");
		}

		private static void DisableSwiftStandardLibraries(this PBXProject project)
		{
			var targetIds = new[]
			{
				project.GetUnityMainTargetGuid(),
				project.GetUnityFrameworkTargetGuid()
			};
			foreach (var targetId in targetIds)
				project.SafeSetBuildProperty(targetId, SWIFT_STANDARD_LIBRARIES, NO);

			Debug.Log($"{nameof(ModifyFrameworksPostProcess)} {nameof(DisableSwiftStandardLibraries)} end");
		}

		private static void DisableBitcode(this PBXProject project)
		{
			var targetIds = new[]
			{
				project.GetUnityMainTargetGuid(),
				project.GetUnityFrameworkTargetGuid()
			};

			foreach (var targetId in targetIds)
				project.SafeSetBuildProperty(targetId, BIT_CODE, NO);

			Debug.Log($"{nameof(ModifyFrameworksPostProcess)} {nameof(DisableBitcode)} end");
		}
	}
}
#endif