using UnityEngine;

namespace RR.Core.Utilities.MaterialPropertyAnimation
{
	public class ColorMaterialAnimator : MaterialAnimator<Color>
	{
		protected override Color GetStartingValue(MeshRenderer meshRenderer, string propertyName)
		{
			return meshRenderer.material.GetColor(propertyName);
		}

		protected override bool HasChanged(Color prevValue, Color currentValue)
		{
			return currentValue != prevValue;
		}

		protected override void ChangeValue(string propertyName, Color value)
		{
			MeshRenderers.ForEach(meshRenderer => meshRenderer.material.SetColor(propertyName, value));
		}
	}
}