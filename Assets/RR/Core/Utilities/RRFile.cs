using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if PLATFORM_STANDALONE_WIN
using System.Diagnostics;
#endif

#if UNITY_2021_2_OR_NEWER
using System.Threading.Tasks;
#endif

namespace RR.Core.Utilities
{
	public static class RRFile
	{
		public static string SaveDataDir = Path.Combine(Application.persistentDataPath, "SaveData");
		public static string ResourcesDataPath = Path.Combine(Application.dataPath, "Resources");

		public static event Action<string> OnFileDeleted;

		public static bool Exists(string filename)
		{
			return File.Exists($"{SaveDataDir}/{filename}");
		}

		public static void Save(string filename, byte[] data)
		{
			try
			{
				CreateDirectoryInSaveData(filename);

				var path = Path.Combine(SaveDataDir, filename);

				RRLogger.Log("Saving Locally. Path: ".Lightblue() + path);
				File.WriteAllBytes(path, data);
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}
#if UNITY_2021_2_OR_NEWER
		public static async Task SaveAsync(string filename, byte[] data)
		{
			try
			{
				CreateDirectoryInSaveData(filename);

				var path = $"{SaveDataDir}/{filename}";

				RRLogger.Log("Saving Locally. Path: ".Lightblue() + path);
				await File.WriteAllBytesAsync(path, data);
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}
#endif
		/// <summary>
		///     Saves local json file
		/// </summary>
		public static void Save(string filename, string contents)
		{
			CreateDirectoryInSaveData(filename);

			var path = $"{SaveDataDir}/{filename}";

			RRLogger.Log("Saving Locally. Path: ".Lightblue() + path);
			File.WriteAllText(path, contents);
		}

		public static void Save<T>(string filename, T contentToSerialize)
			=> Save(filename, JsonConvert.SerializeObject(contentToSerialize));

		public static Texture2D LoadTexture(string filename)
		{
			var path = $"{SaveDataDir}/{filename}";
			if (!File.Exists(path))
				return null;

			var t = new Texture2D(1, 1);
			t.LoadImage(File.ReadAllBytes(path));
			t.Apply();
			return t;
		}

		/// <summary>
		/// Loads local json file
		/// </summary>
		public static JObject LoadJson(string fileName)
		{
			try
			{
				var path = $"{SaveDataDir}/{fileName}";
				if (File.Exists(path))
					return JObject.Parse(File.ReadAllText(path));

				RRLogger.Warning("No file found. " + path);
				return null;
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
				return null;
			}
		}

		/// <summary>
		/// Loads and deserializes local file
		/// </summary>
		public static T Load<T>(string fileName, params JsonConverter[] converts) where T : class
		{
			try
			{
				var path = $"{SaveDataDir}/{fileName}";
				if (File.Exists(path))
				{
					var allText = File.ReadAllText(path);
					return JsonConvert.DeserializeObject<T>(allText, converts);
				}

				RRLogger.Warning("No file found. " + path);
				return null;
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
				return null;
			}
		}

		/// <summary>
		/// Loads local file and returns byte[]
		/// </summary>
		public static byte[] Load(string fileName)
		{
			try
			{
				var path = $"{SaveDataDir}/{fileName}";
				if (File.Exists(path))
				{
					var data = File.ReadAllBytes(path);
					return data;
				}

				RRLogger.Warning("No file found. " + path);
				return null;
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
				return null;
			}
		}
#if UNITY_2021_2_OR_NEWER
		/// <summary>
		/// Loads local file and returns byte[]
		/// </summary>
		public static async Task<byte[]> LoadAsync(string fileName)
		{
			try
			{
				var path = $"{SaveDataDir}/{fileName}";
				if (File.Exists(path))
				{
					var data = await File.ReadAllBytesAsync(path);
					return data;
				}

				RRLogger.Warning("No file found. " + path);
				return null;
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
				return null;
			}
		}
#endif
		/// <summary>
		/// Loads local file and returns a string
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static string TryReadFile(string fileName)
		{
			try
			{
				var path = $"{SaveDataDir}/{fileName}";
				return File.Exists(path) ? File.ReadAllText(path) : null;
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
				return null;
			}
		}

		public static IEnumerable<FileInfo> GetAllSaveFiles(string path = "")
		{
			var finalPath = string.IsNullOrEmpty(path)
				? SaveDataDir
				: Path.Combine(SaveDataDir, path);

			if (!Directory.Exists(finalPath))
				Directory.CreateDirectory(finalPath);

			return new DirectoryInfo(finalPath).GetFiles();
		}

		public static byte[] GetFileData(string filename)
		{
			try
			{
				var path = $"{SaveDataDir}/{filename}";
				return File.Exists(path) ? File.ReadAllBytes(path) : null;
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
				return null;
			}
		}

		/// <summary>
		/// Delete any local file
		/// </summary>
		/// <param name="fileName"></param>
		public static void Delete(string fileName)
		{
			var path = $"{SaveDataDir}/{fileName}";

			if (File.Exists(path))
				File.Delete(path);
			else
				RRLogger.Warning("No file found.");
		}

		/// <summary>
		/// Deletes all files and directories within the SaveData folder
		/// </summary>
		public static void DeleteAllLocal()
		{
			if (!Directory.Exists(SaveDataDir))
				return;

			Directory.Delete(SaveDataDir, true);
			OnFileDeleted?.Invoke("Local Data Deleted");
		}

		/// <summary>
		/// Creates a directory if one doesn't exist
		/// </summary>
		public static void CreateDirectoryInSaveData(string path)
		{
			EnsureSaveDataDirectoryExists();

			var dirPath = Path.GetDirectoryName(Path.GetFullPath(Path.Combine(SaveDataDir, path)));
			if (!Directory.Exists(dirPath) && dirPath != null)
				Directory.CreateDirectory(dirPath);
		}

		public static void EnsureSaveDataDirectoryExists()
		{
			if (!Directory.Exists(SaveDataDir))
				Directory.CreateDirectory(SaveDataDir);
		}

		/// <summary>
		/// Creates new TextAsset in Assets/Resources folder.
		/// EDITOR ONLY
		/// </summary>
		public static TextAsset SaveToTextAsset(string name, string text, string ext = "json")
		{
#if UNITY_EDITOR
			var fileName = $"{name}.{ext}";
			var path = Path.Combine(ResourcesDataPath, fileName);

			new FileInfo(path).Directory.Create(); // ensure directory exist

			if (!File.Exists(path))
				File.CreateText(path).Close();

			File.WriteAllText(path, text);
			AssetDatabase.Refresh();
			return AssetDatabase.LoadAssetAtPath<TextAsset>(Path.Combine("Assets", "Resources", fileName));
#else
			RRLogger.Warning($"Calling {nameof(SaveToTextAsset)} is not supported outside of editor.");
			return null;
#endif
		}

		/// <summary>
		/// Creates new TextAsset in Assets/Resources folder.
		/// EDITOR ONLY
		/// </summary>
		public static TextAsset SaveToTextAsset(string name, object content, string ext = "json",
			Formatting formatting = Formatting.None)
		{
			var text = JsonConvert.SerializeObject(content, formatting);
			return SaveToTextAsset(name, text, ext);
		}

		/// <summary>
		/// Loads from TextAsset in Assets/Resources folder.
		/// </summary>
		public static T LoadFromTextAsset<T>(string relativePath)
		{
			var asset = Resources.Load<TextAsset>(relativePath);
			if (asset == null)
			{
				RRLogger.Error($"Could not find text-asset on relativePath {relativePath}");
				return default;
			}

			var result = JsonConvert.DeserializeObject<T>(asset.text);
			return result;
		}

		/// <summary>
		/// Loads from TextAsset in Assets/Resources folder.
		/// </summary>
		public static string LoadFromTextAsset(string relativePath)
		{
			var asset = Resources.Load<TextAsset>(relativePath);
			if (asset != null)
				return asset.text;

			RRLogger.Error($"Could not find text-asset on relativePath {relativePath}");
			return default;
		}

		public static IEnumerable<T> GetAllFromResources<T>(string relativePath = "") where T : Object
			=> Resources.LoadAll<T>(relativePath);

		/// <summary>
		/// EDITOR ONLY
		/// </summary>
		public static IEnumerable<FileInfo> GetAllFilesFromResources(string relativePath = "", params string[] excludeFileExt)
		{
#if UNITY_EDITOR
			var finalPath = string.IsNullOrEmpty(relativePath)
				? ResourcesDataPath
				: Path.Combine(ResourcesDataPath, relativePath);

			return new DirectoryInfo(finalPath)
				.GetFiles()
				.Where(x => !excludeFileExt.Contains(Path.GetExtension(x.Name)));
#else
			RRLogger.Warning($"Calling {nameof(GetAllFilesFromResources)} is not supported outside of editor.");
			return null;
#endif
		}

		public static void OpenSaveFolder()
		{
#if PLATFORM_STANDALONE_WIN
			Process.Start("explorer.exe", $@"{Path.GetFullPath(SaveDataDir)}");
#elif UNITY_EDITOR
			UnityEditor.EditorUtility.RevealInFinder(Path.GetFullPath(SaveDataDir));
#endif
		}
	}
}
