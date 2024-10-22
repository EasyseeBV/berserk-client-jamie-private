using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BerserkV3.Editor.ArtUploader
{
	public class UploadStoreArtContentsEditor : EditorWindow
	{
		private string uploadEndpoint = "https://berserk-shop-dev.azurewebsites.net/api/v1/File/UploadArtContentsToFileSystem";
		private string uploadSecret = "";
		private string category = "ShopArt";
		private string dataType = "Image";
		private string label = "Exportable";
		private int maxChunkSize = 150;

		private EditorCoroutine mainCoroutine;
		private EditorCoroutine uiUpdateTriggerCoroutine;

		[MenuItem("Tools/Upload PNG Files to Store")]
		public static void ShowWindow()
		{
			GetWindow<UploadStoreArtContentsEditor>("Upload PNG Files to Store");
		}

		private void OnGUI()
		{
			GUILayout.Label("Upload PNG Files from Addressables to Store", EditorStyles.boldLabel);

			uploadEndpoint = EditorGUILayout.TextField("Upload Endpoint", uploadEndpoint);
			uploadSecret = EditorGUILayout.TextField("Upload Secret", uploadSecret);
			category = EditorGUILayout.TextField("Category", category);
			dataType = EditorGUILayout.TextField("Data Type", dataType);
			label = EditorGUILayout.TextField("Addressables Label", label);
			maxChunkSize = EditorGUILayout.IntField("Max Files per Chunk", maxChunkSize);

			if (GUILayout.Button("Upload"))
			{
				uiUpdateTriggerCoroutine = this.StartCoroutine(TriggerUIRefresh());
				mainCoroutine = this.StartCoroutine(UploadAllPngFiles(uiUpdateTriggerCoroutine));
			}

			if (GUILayout.Button("Cancel"))
			{
				if (mainCoroutine != null)
					this.StopCoroutine(mainCoroutine);
				if (uiUpdateTriggerCoroutine != null)
					this.StopCoroutine(uiUpdateTriggerCoroutine);

				mainCoroutine = null;
				uiUpdateTriggerCoroutine = null;
				EditorUtility.ClearProgressBar();
			}
		}

		private IEnumerator UploadAllPngFiles(EditorCoroutine updateTrigger)
		{
			try
			{
				if (string.IsNullOrEmpty(uploadSecret))
					throw new ArgumentOutOfRangeException("Provided secret is empty!");

				Debug.Log("Finding suitable addressables...");

				var filesToUpload = new List<UploadFileModel>();

				var locationsHandle = Addressables.LoadResourceLocationsAsync(label);
				yield return locationsHandle;

				if (locationsHandle.Status == AsyncOperationStatus.Succeeded)
				{
					var locations = locationsHandle.Result;
					Debug.Log($"Found suitable addressables: {locations.Count}");

					var currentChunk = 0;
					for (var i = 0; i < locations.Count; i++)
					{
						if (currentChunk >= maxChunkSize)
						{
							EditorUtility.DisplayProgressBar("Uploading PNG Files",
								$"Reached max chunk size: {currentChunk}/{maxChunkSize} Uploading...", 1f);

							yield return this.StartCoroutine(SendFilesToServer(filesToUpload));
							filesToUpload.Clear();
							currentChunk = 0;
						}

						var location = locations[i];
						EditorUtility.DisplayProgressBar("Preparing PNG Files",
							$"Processed {i} / {locations.Count} keys. Prepared {currentChunk}/{maxChunkSize} chunk files. Current key:{location.PrimaryKey}", (float)i / locations.Count);

						var assetHandle = Addressables.LoadAssetAsync<Sprite>(location);
						yield return assetHandle;

						if (assetHandle.Status == AsyncOperationStatus.Succeeded)
						{
							var asset = assetHandle.Result;

							if (asset != null)
							{
								var rawBytes = ScaleAndConvertTextureToPng(asset.texture, 256, 256);
								var uploadFile = new UploadFileModel
								{
									File = new UploadFileModel.FormFile
									{
										FileName = Path.GetFileName(location.PrimaryKey),
										ContentType = "image/png",
										Data = rawBytes
									},
									Category = category,
									DataType = dataType,
									ArtKey = location.PrimaryKey
								};

								currentChunk += 1;
								filesToUpload.Add(uploadFile);
								Addressables.Release(asset);
							}
						}
					}

					EditorUtility.ClearProgressBar();
				}

				if (filesToUpload.Count > 0)
				{
					EditorUtility.DisplayProgressBar("Uploading PNG Files",
						$"Reached lefover chunk size: {filesToUpload.Count}/{maxChunkSize} Uploading...", 1f);

					yield return this.StartCoroutine(SendFilesToServer(filesToUpload));
				}
				else
				{
					Debug.Log("No PNG files found in the specified Addressables group.");
				}

			}
			finally
			{
				this.StopCoroutine(updateTrigger);
				EditorUtility.ClearProgressBar();
			}
		}

		private IEnumerator SendFilesToServer(List<UploadFileModel> files)
		{
			var form = new WWWForm();

			for (var i = 0; i < files.Count; i++)
			{
				form.AddBinaryData($"Files[{i}].File", files[i].File.Data, files[i].File.FileName, files[i].File.ContentType);
				form.AddField($"Files[{i}].Category", files[i].Category);
				form.AddField($"Files[{i}].DataType", files[i].DataType);
				form.AddField($"Files[{i}].ArtKey", files[i].ArtKey);
			}

			var www = UnityWebRequest.Post(uploadEndpoint, form);
			www.SetRequestHeader("SecretKey",  $"{uploadSecret}");
			www.SetRequestHeader("Authorization", $"Bearer {""}"); // Temp fix to avoid 500 errors from server
			yield return www.SendWebRequest();

			if (www.result != UnityWebRequest.Result.Success)
			{
				 Debug.LogError($"Error uploading files: {www.error}, message: {www.downloadHandler.text}");
				throw new HttpRequestException("Error uploading files.");
			}
			else
			{
				Debug.Log("Files uploaded successfully!");
			}
		}

		private IEnumerator TriggerUIRefresh()
		{
			while (true)
			{
				EditorApplication.QueuePlayerLoopUpdate();
				yield return null;
			}
		}

		private byte[] ScaleAndConvertTextureToPng(Texture2D texture, int maxWidth, int maxHeight)
		{
			var aspectRatio = (float)texture.width / texture.height;
			var newWidth = texture.width;
			var newHeight = texture.height;

			if (texture.width > maxWidth || texture.height > maxHeight)
			{
				if (aspectRatio > 1) // Width is greater than height
				{
					newWidth = maxWidth;
					newHeight = Mathf.RoundToInt(maxWidth / aspectRatio);
				}
				else // Height is greater than width
				{
					newHeight = maxHeight;
					newWidth = Mathf.RoundToInt(maxHeight * aspectRatio);
				}
			}

			var renderTexture = RenderTexture.GetTemporary(newWidth, newHeight, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
			Graphics.Blit(texture, renderTexture);

			var previousRenderTexture = RenderTexture.active;
			RenderTexture.active = renderTexture;

			var scaledTexture = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);
			scaledTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
			scaledTexture.Apply();

			RenderTexture.active = previousRenderTexture;
			RenderTexture.ReleaseTemporary(renderTexture);

			byte[] pngData = scaledTexture.EncodeToPNG();
			DestroyImmediate(scaledTexture);

			return pngData;
		}

		
	}
}