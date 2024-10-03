using System.Collections.Generic;
using RR.Core.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RR.Core.Utilities.MaterialPropertyAnimation
{
	public abstract class MeshRenderChanger : MonoBehaviour
	{
		[SerializeField] protected List<MeshRenderer> MeshRenderers = new();

		[Button]
		public void FindAllMeshRendersByParent()
		{
			MeshRenderers.Clear();
			AddFoundMeshRenders(transform.parent);
		}

		private void AddFoundMeshRenders(Transform transformParent)
		{
			if (transformParent.childCount == 0)
				return;

			transformParent.ForeachChildren<MeshRenderer>(meshRenderer => MeshRenderers.Add(meshRenderer));
			transformParent.ForeachChildren(AddFoundMeshRenders);
		}

		private void OnDestroy()
		{
			//It is your responsibility to destroy the materials when the game object is being destroyed.
			//Resources.UnloadUnusedAssets also destroys the materials but it is usually only called when loading a new level.
			MeshRenderers.ForEach(meshRenderer => Destroy(meshRenderer.material));
		}

		protected virtual void Reset()
		{
			name = $"{transform.parent.name}{GetType().Name}";
			FindAllMeshRendersByParent();
		}
	}
}