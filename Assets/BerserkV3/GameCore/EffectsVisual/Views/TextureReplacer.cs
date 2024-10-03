using System.Collections.Generic;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BerserkV3.GameCore.EffectsVisual
{
	public class TextureReplacer : MonoBehaviour
	{
		[SerializeField] private List<MeshRenderer> meshRenderers = new();
		[SerializeField] private string propertyName = "_CustomTexture";
		[SerializeField] private Texture textureToSet;
		[SerializeField] private Texture defaultTexture;

		[Button]
		public void ResetDefaultTexture()
		{
			meshRenderers.ForEach(meshRenderer => meshRenderer.material.SetTexture(propertyName, defaultTexture));
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

			meshRenderers.ForEach(meshRenderer => meshRenderer.material.SetTexture(propertyName, textureToSet));
		}

		[Button]
		public void FindAllMeshRendersByParent()
		{
			meshRenderers.Clear();
			AddFoundMeshRenders(transform.parent);
		}

		private void AddFoundMeshRenders(Transform transformParent)
		{
			if (transformParent.childCount == 0)
				return;

			transformParent.ForeachChildren<MeshRenderer>(meshRenderer => meshRenderers.Add(meshRenderer));
			transformParent.ForeachChildren(AddFoundMeshRenders);
		}

		private void OnEnable()
		{
			ReplaceCustomTexture();
		}

		private void OnDisable()
		{
			ResetDefaultTexture();
		}

		private void OnDestroy()
		{
			//It is your responsibility to destroy the materials when the game object is being destroyed.
			//Resources.UnloadUnusedAssets also destroys the materials but it is usually only called when loading a new level.
			meshRenderers.ForEach(meshRenderer => Destroy(meshRenderer.material));
		}

		private void Reset()
		{
			name = $"{transform.parent.name}{nameof(TextureReplacer)}";

			if (string.IsNullOrEmpty(propertyName))
				RRLogger.Error($"{nameof(propertyName)} is null or empty");
		}
	}
}