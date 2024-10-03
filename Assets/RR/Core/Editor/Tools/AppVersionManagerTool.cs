using RR.Core.Editor.Extensions;
using RR.Core.Internal;
using UnityEditor;
using UnityEngine;

namespace RR.Core.Editor.Tools
{
	internal class AppVersionManagerTool : EditorWindow
	{
		private string versionInputText;
		private string iosBuildNumberInputText;
		private string androidBundleVersionCodeInputText;
		private SemVer semver;

		[MenuItem(EditorUtils.ComName + "/Version manager", false, 0)]
		private static void ShowWindow()
		{
			GetWindow<AppVersionManagerTool>("Version manager").Show();
		}

		private void OnEnable()
		{
			versionInputText = Application.version;
			iosBuildNumberInputText = PlayerSettings.iOS.buildNumber;
			androidBundleVersionCodeInputText = PlayerSettings.Android.bundleVersionCode.ToString();
		}

		private void OnGUI()
		{
			semver = GetSemVersion(versionInputText);
			versionInputText = semver.ToString();

			var buttonStyle = GUI.skin.button.Extend(style =>
			{
				style.fixedWidth = 70.0f;
				style.fixedHeight = EditorUtils.ElementHeight;
			});

			var labelStyle = GUI.skin.label.Extend(style =>
			{
				style.fixedWidth = 100.0f;
				style.fixedHeight = EditorUtils.ElementHeight;
			});

			GUILayout.Space(EditorUtils.Padding);

			EditorGUILayout.BeginVertical();
			{
				EditorGUILayout.BeginHorizontal();
				{
					versionInputText = EditorGUILayout.TextField(versionInputText,
						GUI.skin.textField.Extend(style => style.fixedHeight = EditorUtils.ElementHeight));

					if (GUILayout.Button("Apply", buttonStyle))
					{
						ApplyNewVersion();
						Close();
					}
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				{
					iosBuildNumberInputText = EditorGUILayout.TextField(iosBuildNumberInputText,
						GUI.skin.textField.Extend(style => { style.fixedHeight = EditorUtils.ElementHeight; }));

					GUILayout.Label("iOS build", labelStyle);
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				{
					androidBundleVersionCodeInputText = EditorGUILayout.TextField(androidBundleVersionCodeInputText,
						GUI.skin.textField.Extend(style => { style.fixedHeight = EditorUtils.ElementHeight; }));

					GUILayout.Label("Android Bundle", labelStyle);
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Major", buttonStyle))
						IncreaseMajor(semver);

					if (GUILayout.Button("Minor", buttonStyle))
						IncreaseMinor(semver);

					if (GUILayout.Button("Patch", buttonStyle))
						IncreasePatch(semver);

					if (GUILayout.Button("Patch IOS", buttonStyle))
						IncreaseIOSPatch();

					GUILayout.FlexibleSpace();
				}
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndVertical();
			GUILayout.Space(EditorUtils.Padding);
		}

		private void IncreaseIOSPatch()
		{
			var iosBuildNumber = int.Parse(iosBuildNumberInputText);
			iosBuildNumber++;
			semver.Patch = 0;
			versionInputText = semver.ToString();
			iosBuildNumberInputText = iosBuildNumber.ToString();
		}

		private void IncreasePatch(SemVer semver)
		{
			semver.Patch += 1;
			versionInputText = semver.ToString();
			IncreaseAndroidBundleVersion();
		}

		private void IncreaseMinor(SemVer semver)
		{
			semver.Minor += 1;
			semver.Patch = 0;
			versionInputText = semver.ToString();
			IncreaseAndroidBundleVersion();
		}

		private void IncreaseMajor(SemVer semver)
		{
			semver.Major += 1;
			semver.Minor = 0;
			semver.Patch = 0;
			versionInputText = semver.ToString();
			IncreaseAndroidBundleVersion();
		}

		private void IncreaseAndroidBundleVersion()
		{
			var bundleVersionCode = int.Parse(androidBundleVersionCodeInputText);
			bundleVersionCode++;
			androidBundleVersionCodeInputText = bundleVersionCode.ToString();
		}

		private void ApplyNewVersion()
		{
			var version = versionInputText;

			PlayerSettings.bundleVersion = version;
			PlayerSettings.macOS.buildNumber = version;
			PlayerSettings.iOS.buildNumber = iosBuildNumberInputText;
			PlayerSettings.Android.bundleVersionCode = int.Parse(androidBundleVersionCodeInputText);

			AssetDatabase.SaveAssets();
		}

		private SemVer GetSemVersion(string versionText)
		{
			return SemVer.TryParse(versionText, out var version)
				? version
				: SemVer.Parse(Application.version);
		}
	}
}