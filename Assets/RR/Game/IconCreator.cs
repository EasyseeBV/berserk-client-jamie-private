using RR.Core.Components;
using RR.Core.DebugSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RR.Game
{
	[RequireComponent(typeof(Camera))]
	public class IconCreator : Singleton<IconCreator>
	{
		private class SpriteData : ObjectData
		{
			public TaskCompletionSource<Sprite> SpriteResult;

			public SpriteData(GameObject go, bool useCaching) : base(go, useCaching)
			{
				SpriteResult = new TaskCompletionSource<Sprite>();
			}
		}
		private class TextureData : ObjectData
		{
			public TaskCompletionSource<Texture2D> TextureResult;

			public TextureData(GameObject go, bool useCaching) : base(go, useCaching)
			{
				TextureResult = new TaskCompletionSource<Texture2D>();
			}
		}

		private class ObjectData
		{
			public GameObject Data;
			public bool UseCaching;

			public ObjectData(GameObject data, bool useCaching)
			{
				Data = data;
				UseCaching = useCaching;
			}
		}

		static string CacheDirectory => Path.Combine(Application.persistentDataPath, "Resources");
		static string GetIconFileName(Object data) => Path.Combine(CacheDirectory, $"{data.name}.png");

		public Camera cam;
		public Transform objectHolder;
		public bool Is2DMode = true;
		public float Padding = 0.03f; //Paddings of image
		public Vector3 Offset = Vector3.zero; // image offset

		const string iconCreatorLayer = "IconCreator";

		Queue<ObjectData> queue = new Queue<ObjectData>();
		bool weAreInWork = false;

		protected override void OnAwake()
		{
			cam.enabled = false;
		}

		/// <summary>Creates Sprite for given 3d Object (or loads from cache)</summary>
		public static async Task<Sprite> CreateSpriteAsync(GameObject go, bool useCache = true)
		{
			//try get cache
			if (useCache)
			{
				var icon = GetSpriteFromCache(go);
				if (icon != null)
					return icon;
			}

			//send to render queue
			var objectData = new SpriteData(go, useCache);
			Instance.queue.Enqueue(objectData);
			return await objectData.SpriteResult.Task;
		}

		/// <summary>Creates Icon for given 3d Object (or loads from cache)</summary>
		public static async Task<Texture2D> CreateTextureAsync(GameObject go, bool useCache = true)
		{
			//try get cache
			if (useCache)
			{
				var icon = GetTextureFromCache(go);
				if (icon != null)
					return icon;
			}

			//send to render queue
			var objectData = new TextureData(go, useCache);
			Instance.queue.Enqueue(objectData);
			return await objectData.TextureResult.Task;
		}

		private static Sprite GetSpriteFromCache(GameObject go)
		{
			var texture = GetTextureFromCache(go);
			if (texture == null)
				return null;

			return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
		}

		private static Texture2D GetTextureFromCache(GameObject go)
		{
			var file = GetIconFileName(go);
			return File.Exists(file)
				? LoadTexture(file)
				: null;
		}

		private void Update()
		{
			if (queue.Count > 0 && !weAreInWork)
			{
				//run coroutine to render icons
				StartCoroutine(CreateIconAsync());
			}
		}

		private IEnumerator CreateIconAsync()
		{
			weAreInWork = true;

			try
			{
				//enumerate items to render
				while (queue.Count > 0)
				{
					var data = queue.Dequeue();
					yield return CreateIconAsync(data);
				}
			}
			finally
			{
				weAreInWork = false;
			}
		}

		private IEnumerator CreateIconAsync(ObjectData data)
		{
			//remove all childs of holder
			while (objectHolder.childCount > 0)
				DestroyImmediate(objectHolder.GetChild(0).gameObject);

			//create clone of object
			var clone = Instantiate(data.Data, objectHolder.transform.position, objectHolder.transform.rotation, objectHolder);

			if (clone == null)
			{
				RRLogger.Error("Doing something wrong. Cant create icon async for providen object.");
				yield break;
			}

			clone.SetActive(true);
			var layer = LayerMask.NameToLayer(iconCreatorLayer);
			if (layer == -1)
				throw new Exception($"Please, create Layer '{iconCreatorLayer}'");
			SetLayerRecursively(clone, layer);

			//calc bounds
			var bounds = GetTotalBounds(clone);
			if (!Is2DMode)
				clone.transform.localPosition -= new Vector3(bounds.center.x, bounds.center.y, 0);
			clone.transform.localPosition += Offset;
			var maxSize = Mathf.Max(bounds.extents.x, bounds.extents.y);

			//render
			cam.enabled = true;
			cam.orthographicSize = maxSize + Padding;
			yield return null;

			//destroy clone
			Destroy(clone);

			//get texture
			var texture = ToTexture2D(cam.targetTexture);

			if (data is TextureData tex)
				tex.TextureResult.SetResult(texture);

			if (data is SpriteData sp)
			{
				var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
				sp.SpriteResult.SetResult(sprite);
			}

			//cam.enabled = false;

			if (!data.UseCaching)
				yield break;

			//save to cache
			if (!Directory.Exists(CacheDirectory))
				Directory.CreateDirectory(CacheDirectory);

			SaveTexture(texture, GetIconFileName(data.Data));
		}

		#region Utils

		static void SaveTexture(Texture2D texture, string filePath)
		{
			var bytes = texture.EncodeToPNG();
			File.WriteAllBytes(filePath, bytes);
		}

		static void SetLayerRecursively(GameObject go, int layerNumber)
		{
			foreach (Transform trans in go.GetComponentsInChildren<Transform>(true))
			{
				trans.gameObject.layer = layerNumber;
			}
		}

		Texture2D ToTexture2D(RenderTexture rTex)
		{
			var tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
			RenderTexture.active = rTex;
			tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
			tex.Apply();
			return tex;
		}

		static Texture2D LoadTexture(string FilePath)
		{
			Texture2D Tex2D;
			byte[] FileData;

			if (File.Exists(FilePath))
			{
				FileData = File.ReadAllBytes(FilePath);
				Tex2D = new Texture2D(2, 2);
				if (Tex2D.LoadImage(FileData))
					return Tex2D;
			}
			return null;
		}

		static Bounds GetTotalBounds(GameObject obj, Func<Renderer, bool> allow = null)
		{
			var rends = obj.GetComponentsInChildren<Renderer>().Where(b => allow == null || allow(b));
			if (!rends.Any())
				return new Bounds(obj.transform.position, new Vector3(1, 1, 1));
			else
			{
				var b = rends.First().bounds;

				foreach (var rend in rends.Skip(1))
				{
					b.Encapsulate(rend.bounds);
				}
				return b;
			}
		}

		#endregion
	}
}
