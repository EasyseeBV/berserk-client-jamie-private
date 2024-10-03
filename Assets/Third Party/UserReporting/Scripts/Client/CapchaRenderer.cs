using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Third_Party.UserReporting.Scripts.Client
{

	public struct Capcha
	{
		public bool IsExpired => isInitialized && expire < DateTime.UtcNow;

		public Texture Texture => texture;

		private readonly bool isInitialized;
		private readonly DateTime expire;
		private RenderTexture texture;
		private string id;

		public Capcha(RenderTexture texture, string id, double lifeTime = 90)
		{
			this.texture = texture;
			this.id = id;
			expire = DateTime.UtcNow + TimeSpan.FromSeconds(lifeTime);
			isInitialized = true;
		}

		public bool Validate(string input)
		{
			return !string.IsNullOrEmpty(input)
			       && !string.IsNullOrEmpty(id)
			       && string.Equals(input, id, StringComparison.CurrentCultureIgnoreCase);
		}

		public void Release()
		{
			if (texture)
				RenderTexture.ReleaseTemporary(texture);

			id = null;
			texture = null;
		}
	}

	public class CapchaRenderer : MonoBehaviour
	{
		[SerializeField] private Camera render;
		[SerializeField] private TextMeshProUGUI input;
		[SerializeField] private Vector2 outputSize;

		public async Task<Capcha> CreateAsync()
		{
			var id = GetId(6);
			var rt = await Render(id);
			return new Capcha(rt, id);
		}

		private async Task<RenderTexture> Render(string id)
		{
			input.text = id;
			var rt = RenderTexture.GetTemporary((int) outputSize.x, (int) outputSize.y);
			render.targetTexture = rt;
			await Task.Delay(TimeSpan.FromSeconds(0.15f));
			render.Render();
			await Task.Delay(TimeSpan.FromSeconds(0.15f));
			render.targetTexture = null;
			return rt;
		}

		private static string GetId(int length)
		{
			return Guid.NewGuid().ToString()[..length].ToUpper();
		}
	}

}