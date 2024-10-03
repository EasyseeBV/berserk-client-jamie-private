using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BerserkV3.GameCore.EffectsVisual.Abstractions
{
	public interface IVfxApplication
	{
		UniTask PlaySingleVFXAsync(object id, params object[] args);
		UniTask PlaySingleVFXAsync(object id, Vector3 position);
		
		/// <summary>
		///  Spawn async vfx to parent if vfx use attach to source, setup default local zero position
		/// </summary>
		/// <param name="id"></param>
		/// <param name="parent">can be used if vfx use attach to source</param>
		/// <returns></returns>
		UniTask PlaySingleVFXAsync(object id, Transform parent);

		VFXView SpawnVfx(object id, params object[] args);
		VFXView SpawnVfx(object id, Vector3 position);
		
		/// <summary>
		/// Spwan vfx to parent if vfx use attach to source, setup default local zero position
		/// </summary>
		/// <param name="id"></param>
		/// <param name="parent">can be used if vfx use attach to source</param>
		/// <returns></returns>
		VFXView SpawnVfx(object id, Transform parent = null);
		
		/// <summary>
		/// Spawn vfx to parent always & setup default local zero position
		/// </summary>
		/// <param name="id"></param>
		/// <param name="parent">always spawn to parent</param>
		/// <returns></returns>
		VFXView SpawnVfxToParent(object id, Transform parent = null);
	}
}