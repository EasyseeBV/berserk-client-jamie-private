using System;
using UnityEngine;

namespace RR.Core.Utilities.MaterialPropertyAnimation
{
	public class FloatMaterialAnimator : MaterialAnimator<float>
	{
		protected override float GetStartingValue(MeshRenderer meshRenderer, string propertyName)
		{
			return meshRenderer.material.GetFloat(propertyName);
		}

		protected override bool HasChanged(float prevValue, float currentValue)
		{
			return Math.Abs(prevValue - currentValue) > 0.01f;
		}

		protected override void ChangeValue(string propertyName, float value)
		{
			MeshRenderers.ForEach(meshRenderer => meshRenderer.material.SetFloat(propertyName, value));
		}
	}
}