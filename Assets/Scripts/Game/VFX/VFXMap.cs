using System.Collections.Generic;
using Berserk.Shared.Data.Enums;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using UnityEngine;
using Vulcan.Data;
using UI;

namespace Vulcan.VFX
{
	[CreateAssetMenu(menuName = "AutoMaps/VFX Map", fileName = "_VFX Map")]
	public sealed class VFXMap : AutoMap<VFXEntity>
	{
		
		[SerializeField] private Dictionary<string, Dictionary<EffectVisualKeyword, VFXEntity>> vfxMap = default;

		public Dictionary<string, Dictionary<EffectVisualKeyword, VFXEntity>> VfxMap => vfxMap;

		public VFXEntity GetVFX(string vfxName)
		{
			if (map.ContainsKey(vfxName)) return map[vfxName];
			return map[VFXKey.None];
		}

		public VFXEntity GetVFX(EffectVisualKeyword effect, string id)
		{
			if (!vfxMap.ContainsKey(id))
				id = VFXKey.Any;

			if (vfxMap[id].ContainsKey(effect))
				return vfxMap[id][effect];

			if (vfxMap[VFXKey.Any].ContainsKey(effect))
				return vfxMap[VFXKey.Any][effect];
		
			return map[VFXKey.None];
		}

		protected override void OnMapped()
		{
			vfxMap.Clear();
			map.Values.ForEach(MapEntity);
		}

		private void MapEntity(VFXEntity entity)
		{
			if (entity.Key.Effect == EffectVisualKeyword.None) return;
			
			if (!vfxMap.ContainsKey(entity.Key.EntityId))
				vfxMap.Add(entity.Key.EntityId, new Dictionary<EffectVisualKeyword, VFXEntity>());
			
			if (!vfxMap[entity.Key.EntityId].ContainsKey(entity.Key.Effect))
			{
				vfxMap[entity.Key.EntityId].Add(entity.Key.Effect, entity);
				return;
			}
			
			RRLogger.Error($"[{"VFX MAP".Maroon().Bold()}] Key {entity.Key} has already been added!");
		}
	}

}