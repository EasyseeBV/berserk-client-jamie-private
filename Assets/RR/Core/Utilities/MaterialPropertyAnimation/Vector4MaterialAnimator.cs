using UnityEngine;

namespace RR.Core.Utilities.MaterialPropertyAnimation
{
	public class Vector4MaterialAnimator : MaterialAnimator<Vector4>
	{
		protected override Vector4 GetStartingValue(MeshRenderer meshRenderer, string propertyName)
		{
			return meshRenderer.material.GetVector(propertyName);
		}

		protected override bool HasChanged(Vector4 prevValue, Vector4 currentValue)
		{
			return currentValue != prevValue;
		}

		protected override void ChangeValue(string propertyName, Vector4 value)
		{
			MeshRenderers.ForEach(meshRenderer => meshRenderer.material.SetVector(propertyName, value));
		}
	}
}