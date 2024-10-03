using System;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Generic.Emotions
{
	public class UIFlipBook : IDisposable
	{
		private RawImage image;
		private float width;
		private float height;
		private float frameRate;
		private float timeScale;
		private float totalFrames;
		private Vector2 frameSize;

		public UIFlipBook(RawImage image)
		{
			this.image = image;
		}
	
		public void Init(float width, float height, float timeScale, float frameRate = 30)
		{
			if(!image) 
				return;
		
			this.width = width;
			this.height = height;
			this.timeScale = timeScale;
			this.frameRate = frameRate;
		
			totalFrames = width * height;
			frameSize = new Vector2(1 / width, 1 / height);
		
			var rect = image.uvRect;
			rect.size = frameSize;
			image.uvRect = rect;
		}

		public void Animate()
		{
			if(!image)
				return;
		
			var currentFrame = (Time.time * timeScale * frameRate) % totalFrames;
			var rect = image.uvRect;
			rect.position = GetTwoDimensionIndex(currentFrame) * frameSize;
			image.uvRect = rect;
		}

		public void Reset()
		{
			if(!image)
				return;
			image.uvRect = new Rect(Vector2.zero,Vector2.one);
		}

		private Vector2 GetTwoDimensionIndex(float currentFrame)
		{
			var position = currentFrame / width;
			var y = (float) Math.Truncate(position);
			var x = Mathf.Floor((position - y) / (1f / width));
			return new Vector2(x, height - y);
		}

		public void Dispose()
		{
			image = null;
		}
	}
}