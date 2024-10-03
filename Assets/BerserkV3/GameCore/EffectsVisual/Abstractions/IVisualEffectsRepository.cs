using System.Collections.Generic;
using BerserkV3.GameCore.EffectsVisual.Visuals;

namespace BerserkV3.GameCore.EffectsVisual.Abstractions
{
	public interface IVisualEffectsRepository
	{
		IEnumerable<EffectVisual> GetAll();
		IEnumerable<EffectVisual> GetMany(IEnumerable<int> ids);
		bool TryGet(int id, out EffectVisual result);
		void Remove(int id);
		bool TryAdd(EffectVisual effectVisual);
	}
}