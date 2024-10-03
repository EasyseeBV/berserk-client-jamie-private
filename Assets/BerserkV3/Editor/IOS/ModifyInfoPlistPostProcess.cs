#if UNITY_IOS || UNITY_TVOS

using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace BerserkV3.Editor.IOS
{
	public static class ModifyInfoPlistPostProcess
	{
		[PostProcessBuild]
		public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
		{
			if (target != BuildTarget.iOS)
				return;

			try
			{
				Debug.Log($"{nameof(ModifyInfoPlistPostProcess)} Start Post Process Build");

				var plistPath = pathToBuiltProject + "/Info.plist";
				var plist = new PlistDocument();
				plist.ReadFromString(File.ReadAllText(plistPath));

				var rootDict = plist.root;
				rootDict.SetString("NSUserTrackingUsageDescription",
					"Your data will be used to provide you with a better gaming experience.");
				Debug.Log("Info.plist updated with NSUserTrackingUsageDescription");

				rootDict.SetString("NSAdvertisingAttributionReportEndpoint", "https://appsflyer-skadnetwork.com/");
				Debug.Log("Info.plist updated with NSAdvertisingAttributionReportEndpoint");

				File.WriteAllText(plistPath, plist.WriteToString());

				Debug.Log($"{nameof(ModifyInfoPlistPostProcess)} End success Post Process Build");
			}
			catch (Exception e)
			{
				Debug.LogError(
					$"{nameof(ModifyInfoPlistPostProcess)} End failed Post Process Build. Message: {e.Message}");
				Debug.LogException(e);
				throw;
			}
		}
	}
}
#endif