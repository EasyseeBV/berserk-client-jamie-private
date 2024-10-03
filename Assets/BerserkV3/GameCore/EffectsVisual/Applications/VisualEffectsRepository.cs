using System;
using System.Collections.Generic;
using System.Linq;
using BerserkV3.GameCore.EffectsVisual.Abstractions;
using BerserkV3.GameCore.EffectsVisual.Visuals;
using RR.Core.Extensions;

namespace BerserkV3.GameCore.EffectsVisual.Applications
{
	public class VisualEffectsRepository : IVisualEffectsRepository, IDisposable
	{
		private readonly Dictionary<int, EffectVisual> effects = new();

		public void Dispose()
		{
			effects.Values.ForEach(x => x?.Dispose());
			effects.Clear();
		}

		public IEnumerable<EffectVisual> GetAll()
		{
			return effects.Values.ToArray();
		}

		public IEnumerable<EffectVisual> GetMany(IEnumerable<int> ids)
		{
			return ids?
				.Select(id => TryGet(id, out var effectVisual) ? effectVisual : null)
				.Where(x => x != null)
				.ToArray() ?? Array.Empty<EffectVisual>();
		}

		public bool TryGet(int id, out EffectVisual result)
		{
			return effects.TryGetValue(id, out result);
		}

		public void Remove(int id)
		{
			effects.Remove(id);
		}

		public bool TryAdd(EffectVisual effectVisual)
		{
			return effectVisual == null || effects.TryAdd(effectVisual.Model.Id, effectVisual);
		}
	}
}