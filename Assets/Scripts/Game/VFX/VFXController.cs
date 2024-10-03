using RR.Core.Components;
using Vulcan.Data;
using UnityEngine;
using Lean.Pool;
using Events;
using System;
using Game.Entities;
using System.Linq;
using Berserk.Shared.Data.Enums;
using RR.Core.DebugSystem;
using RR.Core.Extensions;

namespace Vulcan.VFX
{
	public class VFXController : Singleton<VFXController>
	{
		[SerializeField] private VFXMap vfxMap = default;

		public static void Spawn(Vector3 position, EffectVisualKeyword effect, string id = "")
		{
			var vfx = Instance.vfxMap.GetVFX(effect, id);

			if (vfx == null)
			{
				RRLogger.Error($"Can't find vfx for effect {effect}. Possible vfx map error");
				return;
			}
			
			var vfxEntity = LeanPool.Spawn(vfx);
			
			if (vfxEntity == null)
			{
				RRLogger.Error($"Can't spawn vfx entity for effect {effect}. Possible vfx map error");
				return;
			}

			vfxEntity
				.SetPosition(position)
				.Initialize()
				.Play();
		}

		/// <summary>
		/// Spawn vfx with in world position from EffectInfo.From.
		/// Can attach to parent EffectInfo.From if effect use AttachToSource.
		/// Setup target in world position from EffectInfo.Target.
		/// Best suited for projectile effects (From => To).
		/// VFX cannot be spawned if any entity destroyed or target entity alredy died.
		/// </summary>
		/// <param name="effect"></param>
		/// <param name="deSpawnFunc"> can overload despawn function to control vfx lifetime</param>
		public static void SpawnProjectile(EffectInfo effect, Func<bool> deSpawnFunc = null)
		{
			if (effect.From == null
			    || effect.Target == null
			    || effect.Target.DataBase.Hp <= 0)
			{
				return;
			}

			var vfx = Instance.vfxMap.GetVFX(effect.VisualEffect, effect.Target.DataBase.Id);
			if (vfx == null)
				return;

			var parent = vfx.AttachToSource ? effect.From.RectTransform : null;

			deSpawnFunc ??= () => !effect.From.DataBase.EffectsContainer.Has(effect.Effect);
			LeanPool.Spawn(vfx, parent)
				.SetPosition(effect.From.RectTransform.position)
				.SetTargetPosition(effect.Target.RectTransform.position)
				.Initialize()
				.Play(deSpawnFunc);
		}

		public static void Spawn(EffectInfo effect, Func<bool> deSpawnFunc = null, bool isRemote = false)
		{
			if (effect.Target.DataBase.Hp <= 0)
				return;

			var vfx = Instance.vfxMap.GetVFX(effect.VisualEffect, effect.Target.DataBase.Id);

			if (vfx == null)
			{
				Debug.LogError($"VFX not found for {effect.VisualEffect}. All: {string.Join(",", Instance.vfxMap.VfxMap.Keys)}");
				return;
			}

			if (isRemote && vfx.IsOneShot)
				return;

			var parent = vfx.AttachToSource
				? effect.From
				: effect.Target;
			if (parent?.RectTransform.Value() == null)
			{
				RRLogger.Error($"[{"VFXControler".Red().Bold()}] Parent.RectTransform is missing" +
				               $" : VisualEffect : {effect.VisualEffect}, Effect : {effect.Effect}, isRemote : {isRemote}");
				return;
			}

			// check if alredy spawned current effect
			var vfxEntities = parent.RectTransform.GetComponentsInChildren<VFXEntity>(); 
			if (vfxEntities.Any(x => x.Key.Effect == effect.VisualEffect))
				return;
			deSpawnFunc ??= () => !parent.DataBase.EffectsContainer.Has(effect.Effect);

			LeanPool.Spawn(vfx, parent.RectTransform)
				.SetTargetPosition(effect.Target.RectTransform.position)
				.SetLocalPosition(Vector3.zero)
				.Initialize()
				.Play(deSpawnFunc);
		}

		/// <summary>
		/// Spawn effect. Target should contains effect.
		/// </summary>
		/// <param name="effect"></param>
		/// <param name="target"></param>
		/// <param name="deSpawnFunc"></param>
		public static void Spawn(EffectKeyword effect, IMonoEntity target, Func<bool> deSpawnFunc = null)
		{
			if (target.DataBase.Hp <= 0)
				return;

			deSpawnFunc ??= () => !target.DataBase.EffectsContainer.Has(effect);

			var vfx = Instance.vfxMap.GetVFX(target.DataBase.EffectsContainer.GetOrNull(effect).EffectData.VisualEffect,
			                                 target.DataBase.Id);

			if (vfx == null)
			{
				Debug.LogError($"VFX not found for {effect}. All: {string.Join(",", Instance.vfxMap.VfxMap.Keys)}");
				return;
			}

			var vfxEntity = LeanPool.Spawn(vfx, target.RectTransform);
			vfxEntity
				.SetLocalPosition(Vector3.zero)
				.Initialize()
				.Play(deSpawnFunc);
		}

		public static void Spawn(EffectVisualKeyword visualEffect, IMonoEntity target, Func<bool> deSpawnFunc)
		{
			if (target.DataBase.Hp <= 0)
				return;

			var vfx = Instance.vfxMap.GetVFX(visualEffect, target.DataBase.Id);

			if (vfx == null)
			{
				Debug.LogError($"VFX not found for {visualEffect}. All: {string.Join(",", Instance.vfxMap.VfxMap.Keys)}");
				return;
			}

			var vfxEntity = LeanPool.Spawn(vfx, target.RectTransform);
			vfxEntity
				.SetLocalPosition(Vector3.zero)
				.Initialize()
				.Play(deSpawnFunc);
		}
	}
}