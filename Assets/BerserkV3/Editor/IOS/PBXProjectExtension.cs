#if UNITY_IOS || UNITY_TVOS

using System;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace BerserkV3.Editor.IOS
{
	public static class PBXProjectExtension
	{
		public static void SafeSetBuildProperty(this PBXProject project, string targetId, string name, string value)
		{
			try
			{
				if (string.IsNullOrEmpty(targetId))
					throw new ArgumentNullException($"{nameof(targetId)}");

				project.SetBuildProperty(targetId, name, value);
				Debug.Log($"{nameof(ModifyFrameworksPostProcess)} Set {name} to {value} for target {targetId}");
			}
			catch (Exception e)
			{
				Debug.LogError(
					$"{nameof(ModifyFrameworksPostProcess)} {nameof(SafeSetBuildProperty)} {name} for {targetId} failed! {e.Message}");
				Debug.LogException(e);
			}
		}
	}
}
#endif