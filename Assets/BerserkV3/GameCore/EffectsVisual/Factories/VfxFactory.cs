using System;
using System.Collections.Generic;
using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.GameCore.EffectsVisual.Abstractions;
using RR.Core.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BerserkV3.GameCore.EffectsVisual.Factories
{
	public class VfxFactory : IVfxFactory, IDisposable
	{
		private const string VFX_VISUALS_PATH = "VFXPrefabs/{0}";
		private readonly Dictionary<string, VFXView> loadedPrefabsPool = new();

		public VFXView Create(string visualKeyword, Transform parent = null)
		{
			if (!loadedPrefabsPool.TryGetValue(visualKeyword, out var prefab))
			{
				prefab = Resources.Load<VFXView>(string.Format(VFX_VISUALS_PATH, visualKeyword));

				if (prefab == null)
				{
					if (visualKeyword == "None")
						throw new InvalidOperationException($"VFXView not found : {visualKeyword.Red()}");

					prefab = Fallback(visualKeyword);
				}

				loadedPrefabsPool[visualKeyword] = prefab;
			}

			return Object.Instantiate(prefab, parent);
		}

		private VFXView Fallback(string visualKeyword)
		{
			DefaultSharedLogger.Log($"VFXView not found : {visualKeyword.Red()}, used fallback view : EffectVisualKeyword.None");
			return Create("None");
		}

		public void Dispose()
		{
			loadedPrefabsPool.Clear();
		}
	}
}