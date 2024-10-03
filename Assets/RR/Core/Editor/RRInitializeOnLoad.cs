using RR.Core.DebugSystem;
using UnityEditor;

namespace RR.Core.Editor
{
	[InitializeOnLoad]
	public class RRInitializeOnLoad
	{
		static RRInitializeOnLoad()
		{
			if (PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Android) != ApiCompatibilityLevel.NET_4_6)
			{
				RRLogger.Log("Api compatibility level has changed to <b>NET 4.6</b> for Android");
				PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_4_6);
			}

			if (PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Standalone) != ApiCompatibilityLevel.NET_4_6)
			{
				RRLogger.Log("Api compatibility level has changed to <b>NET 4.6</b> for Standalone");
				PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_4_6);
			}
			
			if (PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.WebGL) != ApiCompatibilityLevel.NET_4_6)
			{
				RRLogger.Log($"Api compatibility level has changed to <b>NET 4.6</b> for {BuildTarget.WebGL}");
				PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.WebGL, ApiCompatibilityLevel.NET_4_6);
			}
		}
	}
}
