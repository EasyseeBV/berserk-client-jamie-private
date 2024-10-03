using UnityEngine;

namespace BerserkV3.GameCore.EffectsVisual.Abstractions
{
	public interface IVfxFactory
	{
		VFXView Create(string id, Transform parent = null);
	}
}