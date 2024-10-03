using RR.Core.DebugSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RR.Core.Utilities.MaterialPropertyAnimation
{
	public class TextureReplacer : MeshRenderChanger
	{
		[SerializeField] private string propertyName = "_CustomTexture";
		[SerializeField] private Texture textureToSet;
		[SerializeField] private Texture defaultTexture;

		[Button]
		public void ResetDefaultTexture()
		{
			MeshRenderers.ForEach(meshRenderer => meshRenderer.material.SetTexture(propertyName, defaultTexture));
		}

		[Button]
		public void ReplaceCustomTexture()
		{
			if (textureToSet == null)
			{
				RRLogger.Error($"{nameof(textureToSet)} not found by {name}");
				return;
			}

			if (string.IsNullOrEmpty(propertyName))
			{
				RRLogger.Error($"{nameof(propertyName)} is null or empty");
				return;
			}

			MeshRenderers.ForEach(meshRenderer => meshRenderer.material.SetTexture(propertyName, textureToSet));
		}

		private void OnEnable()
		{
			ReplaceCustomTexture();
		}

		private void OnDisable()
		{
			ResetDefaultTexture();
		}

#if UNITY_EDITOR
		private void Reset()
		{
			name = $"{transform.parent.name}{nameof(TextureReplacer)}";

			if (string.IsNullOrEmpty(propertyName))
				RRLogger.Error($"{nameof(propertyName)} is null or empty");
		}
#endif
	}
}