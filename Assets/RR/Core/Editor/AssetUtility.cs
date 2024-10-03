using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RR.Core.Editor
{
	[InitializeOnLoad]
	public static class AssetUtility
	{

#if RR_ASSETBUS
		static AssetUtility()
		{
			AssetProcessorBus.ReceiveAssetsFromDb.HideInLog =
				AssetProcessorBus.RequestAssetsFromDb.HideInLog = true;

			AssetProcessorBus.RequestAssetsFromDb.SubscribeRaw(x =>
			{
				var assets = GetAllAssets(x).Select(a => a.asset);
				AssetProcessorBus.ReceiveAssetsFromDb += assets;
			});
		}
#endif

		public static string MakePathRelative(string absolutePath) => absolutePath.Replace(@"\", "/").Replace(Application.dataPath, "Assets");

		public static T LoadAssetAtAbsolutePath<T>(string absolutePath) where T : Object
		{
			return AssetDatabase.LoadAssetAtPath<T>(MakePathRelative(absolutePath));
		}

		public static Object LoadAssetAtAbsolutePath(string absolutePath, Type type)
		{
			return AssetDatabase.LoadAssetAtPath(MakePathRelative(absolutePath), type);
		}

		public static IEnumerable<T> LoadAssetsAtAbsolutePath<T>(string[] absolutePath) where T : Object
		{
			return absolutePath.Select(x => AssetDatabase.LoadAssetAtPath<T>(MakePathRelative(x)));
		}

		public static FoundAsset<Object>[] GetAllAssets(Type type)
		{
			return new DirectoryInfo(Application.dataPath)
				.GetFiles("*.prefab", SearchOption.AllDirectories)
				.Select(file => file.FullName)
				.Select(path => new FoundAsset<Object>(LoadAssetAtAbsolutePath(path, type), path))
				.Where(foundAsset => foundAsset.asset != null)
				.ToArray();
		}

		public static FoundAsset<T>[] GetAllAssets<T>() where T : Object
		{
			return new DirectoryInfo(Application.dataPath)
				.GetFiles("*.prefab", SearchOption.AllDirectories)
				.Select(file => file.FullName)
				.Select(path => new FoundAsset<T>(LoadAssetAtAbsolutePath<T>(path), path))
				.Where(foundAsset => foundAsset.asset != null)
				.ToArray();
		}

		public static FoundAsset<T>[] GetAssetsAtRelativePath<T>(string path, SearchOption searchOption = SearchOption.AllDirectories) where T : Object
		{
			return new DirectoryInfo(Path.Combine(Application.dataPath, path))
				.GetFiles("*.prefab", searchOption)
				.Select(file => file.FullName)
				.Select(absolutePath => new FoundAsset<T>(LoadAssetAtAbsolutePath<T>(absolutePath), absolutePath))
				.Where(foundAsset => foundAsset.asset != null)
				.ToArray();
		}

		public static FoundAsset<Object>[] GetAssetsAtRelativePath(string path, SearchOption searchOption = SearchOption.AllDirectories)
		{
			return new DirectoryInfo(Path.Combine(Application.dataPath, path))
				.GetFiles("*.prefab", searchOption)
				.Select(file => file.FullName)
				.Select(absolutePath => new FoundAsset<Object>(LoadAssetAtAbsolutePath(absolutePath, typeof(Object)), absolutePath))
				.Where(foundAsset => foundAsset.asset != null)
				.ToArray();
		}

		public static T[] FindPrefabs<T>() where T : Object => FindAssetsWithExtension<T>(".prefab");
		public static T[] FindAssetsWithExtension<T>(string fileExtension) where T : Object
		{
			var paths = FindAssetPathsWithExtension(fileExtension);
			if (paths == null || paths.Length == 0)
				return null;

			return paths
				.Select(path => AssetDatabase.LoadAssetAtPath(path, typeof(T)) as T)
				.Where(asset => asset != null)
				.ToArray();
		}

		public static string[] FindAssetPathsWithExtension(string fileExtension)
		{
			if (string.IsNullOrEmpty(fileExtension))
				return null;

			fileExtension = fileExtension.TrimStart('.');

			var directoryInfo = new DirectoryInfo(Application.dataPath);
			var fileInfos = directoryInfo.GetFiles("*." + fileExtension, SearchOption.AllDirectories);

			return fileInfos.Select(file => file.FullName.Replace(@"\", "/").Replace(Application.dataPath, "Assets")).ToArray();
		}

		public static T CreateUIElementFromPrefab<T>(string prefabPath, string prefabName = "", bool attachToCanvas = true) where T : Component
		{
			var prefab = AssetDatabase.LoadAssetAtPath<T>(prefabPath);
			var instance = Object.Instantiate(prefab);

			instance.name = string.IsNullOrEmpty(prefabName) ? instance.name : prefabName;

			Undo.RegisterCreatedObjectUndo(instance, "Create " + instance.name);

			if (attachToCanvas)
			{
				var ignoredCanvasNames = new[] { "RR Console" };
				var selectionRoot = Selection.activeGameObject?.transform.GetComponentInParent<Canvas>();

				if (ignoredCanvasNames.Contains(selectionRoot?.name))
					selectionRoot = null;

				var target = (selectionRoot != null
								 ? Selection.activeGameObject.transform
								 : Object.FindObjectsOfType<Canvas>()
									 .FirstOrDefault(x => !ignoredCanvasNames.Contains(x.name))?.transform)
							 ?? CreateNewCanvas().transform;

				instance.transform.SetParent(target);
				instance.transform.localPosition = Vector3.zero;
			}

			Selection.activeObject = instance;
			Selection.selectionChanged?.Invoke();

			return instance;
		}

		private static Canvas CreateNewCanvas()
		{
			var cv = new GameObject("Canvas",
					typeof(Canvas),
					typeof(GraphicRaycaster),
					typeof(CanvasScaler))
				.GetComponent<Canvas>();

			cv.sortingOrder = 100;
			cv.renderMode = RenderMode.ScreenSpaceOverlay;

			var sc = cv.GetComponent<CanvasScaler>();
			sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			sc.referenceResolution = new Vector2(1920, 1080);
			sc.matchWidthOrHeight = 0.5f;

			return cv;
		}
	}

	public class FoundAsset<T>
	{
		public T asset { get; }
		public string path { get; }

		public FoundAsset(T asset, string path)
		{
			this.asset = asset;
			this.path = path;
		}
	}
}
