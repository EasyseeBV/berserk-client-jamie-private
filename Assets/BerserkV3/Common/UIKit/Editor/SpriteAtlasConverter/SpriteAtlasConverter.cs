using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace BerserkV3.Common.UIKit.Editor.SpriteAtlasConverter
{
	/// <summary>
	/// Taken from: https://forum.unity.com/threads/text-mesh-pro-does-not-work-with-spriteatlas-assets.697088/#post-7581571
	/// All credits go to the original author
	/// Original name: SimpleSpriteAtlasConverter
	/// Atlas allowRotation and tightPacking must be set to false.
	/// Select a Sprite atlas and go to "Tools/UITools/Repack atlas to sprite" to convert it to a sprite.
	/// This is most useful to add a texture to TMPro without manual atlas packing via third party tools
	/// </summary>
	public class SpriteAtlasConverter
	{
		[MenuItem("Tools/UITools/Repack atlas to sprite", true)]
		private static bool RepackAtlasToSprite()
		{
			if (Selection.activeObject == null)
			{
				Debug.Log("No object selected");
				return false;
			}

			return Selection.activeObject.GetType() == typeof(SpriteAtlas);
		}

		[MenuItem("Tools/UITools/Repack atlas to sprite")]
		static void RepackAtlasToSprite(MenuCommand command)
		{
			SpriteAtlas atlas = (SpriteAtlas)Selection.activeObject;

			if (atlas.GetPackingSettings().enableRotation || atlas.GetPackingSettings().enableTightPacking)
			{
				Debug.LogError($"Problem! Atlas 'allowRotation' and 'tightPacking' must be set to false in '{Selection.activeObject.name}' to work correctly!", Selection.activeObject);
				return;
			}

			var padding = atlas.GetPackingSettings().padding;

			//Pack sprites
			Sprite[] sprites = new Sprite[atlas.spriteCount];
			atlas.GetSprites(sprites);
			var rects = new List<SpriteRect>();
			foreach (var spr in sprites)
			{
				rects.Add(new SpriteRect(spr, padding));
			}

			rects.Sort((a, b) => b.area.CompareTo(a.area));

			var packer = new RectanglePacker();

			foreach (var rect in rects)
			{
				if (!packer.Pack(rect.w, rect.h, out rect.x, out rect.y))
					throw new Exception("Uh oh, we couldn't pack the rectangle :(");
			}

			//Calculate image size
			var maxSize = atlas.GetPlatformSettings("DefaultTexturePlatform").maxTextureSize;
			var pngSize = Math.Max(packer.Width, packer.Height);
			var powoftwo = 16;
			while (powoftwo < pngSize) powoftwo *= 2;
			pngSize = powoftwo;
			if (pngSize > maxSize) pngSize = maxSize;

			// LL added: seems like some folks need sRGB, so we add a choice for that
			var optionLinear = RenderTextureReadWrite.Linear;
			var optionSrgb = RenderTextureReadWrite.sRGB;
			var option = EditorUtility.DisplayDialogComplex(
				"Choose texture format",
				"This will be the target texture format. If in doubt, use Linear.",
				optionLinear.ToString(),
				optionSrgb.ToString(),
				"Cancel"
			);
			RenderTextureReadWrite renderTextureReadWrite;
			if (option == 0)
				renderTextureReadWrite = optionLinear;
			else if (option == 1)
				renderTextureReadWrite = optionSrgb;
			else
			{
				Debug.Log("Cancelled packing texture");
				return;
			}

			Texture2D texture = new Texture2D(pngSize, pngSize, TextureFormat.RGBA32, false);

			//Make texture transparent
			Color fillColor = Color.clear;
			Color[] fillPixels = new Color[texture.width * texture.height];
			for (int i = 0; i < fillPixels.Length; i++) fillPixels[i] = fillColor;
			texture.SetPixels(fillPixels);

			var metas = new List<SpriteMetaData>();

			//Draw sprites
			foreach (var rect in rects)
			{
				var t = GetReadableTexture(rect.sprite.texture, renderTextureReadWrite);
				texture.SetPixels32(rect.x + padding, rect.y + padding, (int)rect.sprite.rect.width, (int)rect.sprite.rect.height, t.GetPixels32());
				metas.Add(new SpriteMetaData()
				{
					alignment = 6, //BottomLeft
					name = rect.sprite.name.Replace("(Clone)", ""),
					rect = new Rect(rect.x + padding, rect.y + padding, rect.sw, rect.sh)
				});
			}

			//Save image
			var path = AssetDatabase.GetAssetPath(atlas);

			// LL: handling Sprite Atlas V2 case
			string pngPath;
			if (path.EndsWith(".spriteatlas"))
				pngPath = path.Replace(".spriteatlas", ".png");
			else if (path.EndsWith(".spriteatlasv2"))
				pngPath = path.Replace(".spriteatlasv2", ".png");
			else
				throw new InvalidOperationException($"Unexpected file extension: {path}");

			Debug.Log($"Create sprite from atlas: {atlas.name} path: {path}");

			byte[] bytes = texture.EncodeToPNG();
			var tempPngPath = pngPath; //To prevent bug in number of naming
			int j = 1;
			while (File.Exists(tempPngPath))
			{
				tempPngPath = Path.Combine(Path.GetDirectoryName(pngPath), $"{Path.GetFileNameWithoutExtension(pngPath)} {j}{Path.GetExtension(pngPath)}");
				j++;
			}

			pngPath = tempPngPath;
			File.WriteAllBytes(pngPath, bytes);

			//Update sprite settings
			AssetDatabase.Refresh();

			TextureImporter ti = AssetImporter.GetAtPath(pngPath) as TextureImporter;
			ti.textureType = TextureImporterType.Sprite;
			ti.spriteImportMode = SpriteImportMode.Multiple;
			ti.spritesheet = metas.ToArray();

			EditorUtility.SetDirty(ti);
			ti.SaveAndReimport();
		}

		private static Texture2D GetReadableTexture(Texture2D source, RenderTextureReadWrite readWriteMode)
		{
			RenderTexture tmp = RenderTexture.GetTemporary(
				source.width,
				source.height,
				0,
				RenderTextureFormat.ARGB32,
				readWriteMode);

			Graphics.Blit(source, tmp);
			RenderTexture previous = RenderTexture.active;
			RenderTexture.active = tmp;
			Texture2D result = new Texture2D(source.width, source.height);
			result.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
			result.Apply();
			RenderTexture.active = previous;
			RenderTexture.ReleaseTemporary(tmp);
			return result;
		}
	}

	public class SpriteRect
	{
		public Sprite sprite;
		public int x, y, w, h;

		public SpriteRect(Sprite sprite, int padding)
		{
			this.sprite = sprite;
			this.x = 0;
			this.y = 0;
			this.w = (int)sw + padding * 2;
			this.h = (int)sh + padding * 2;
		}

		public int sw => (int)sprite.rect.width;
		public int sh => (int)sprite.rect.height;
		public int area => w * h;
	}

	// https://github.com/mikaturunen/RectanglePacker
	public class RectanglePacker
	{
		public int Width { get; private set; }
		public int Height { get; private set; }

		List<Node> nodes = new List<Node>();

		public RectanglePacker()
		{
			nodes.Add(new Node(0, 0, int.MaxValue, int.MaxValue));
		}

		public bool Pack(int w, int h, out int x, out int y)
		{
			for (int i = 0; i < nodes.Count; ++i)
			{
				if (w <= nodes[i].W && h <= nodes[i].H)
				{
					var node = nodes[i];
					nodes.RemoveAt(i);
					x = node.X;
					y = node.Y;
					int r = x + w;
					int b = y + h;
					nodes.Add(new Node(r, y, node.Right - r, h));
					nodes.Add(new Node(x, b, w, node.Bottom - b));
					nodes.Add(new Node(r, b, node.Right - r, node.Bottom - b));
					Width = Math.Max(Width, r);
					Height = Math.Max(Height, b);
					return true;
				}
			}

			x = 0;
			y = 0;
			return false;
		}

		public struct Node
		{
			public int X;
			public int Y;
			public int W;
			public int H;

			public Node(int x, int y, int w, int h)
			{
				X = x;
				Y = y;
				W = w;
				H = h;
			}

			public int Right
			{
				get { return X + W; }
			}

			public int Bottom
			{
				get { return Y + H; }
			}
		}
	}
}