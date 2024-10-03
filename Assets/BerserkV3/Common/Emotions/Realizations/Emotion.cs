using System;

namespace BerserkV3.Generic.Emotions
{
	public class Emotion
	{
		public string Id { get; }
		public float LifeTime { get; }
		public float TimeScale { get; }
		public int Width { get; }
		public int Height { get; }
		public string Uri { get; }

		public Emotion(string id, string uri, int height, int width, float timeScale, float lifeTime)
		{
			if (string.IsNullOrEmpty(id))
				throw new NullReferenceException($"{nameof(id)} cannot be {nameof(string.IsNullOrEmpty)}");

			if (string.IsNullOrEmpty(uri))
				throw new NullReferenceException($"{nameof(uri)} cannot be {nameof(string.IsNullOrEmpty)} by id: {id}");

			if (height < 0)
				throw new ArgumentOutOfRangeException($"{nameof(height)} cannot be < 0 by id: {id}");

			if (width < 0)
				throw new ArgumentOutOfRangeException($"{nameof(width)} cannot be < 0 by id: {id}");

			Id = id;
			Uri = uri;
			Height = height;
			Width = width;
			TimeScale = timeScale;
			LifeTime = lifeTime;
		}
	}
}