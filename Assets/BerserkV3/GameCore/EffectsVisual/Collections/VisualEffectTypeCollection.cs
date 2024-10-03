using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.EffectsVisual.Abstractions;
using BerserkV3.GameCore.EffectsVisual.Attributes;
using BerserkV3.GameCore.EffectsVisual.Visuals;
using RR.Core.Extensions;
using Sirenix.Utilities;
using Zenject;

namespace BerserkV3.GameCore.EffectsVisual.Collections
{
	public class VisualEffectTypeCollection : IVisualEffectTypeCollection, IInitializable, IDisposable
	{
		private Dictionary<EffectVisualKeyword, Type> typeStorage;
		public void Initialize()
		{
			var effectVisualSubclassTypes = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(x => x.IsClass
				            && !x.IsAbstract
				            && x.IsSubclassOf(typeof(EffectVisual))
				            && x.CustomAttributes.Any());

			typeStorage = new Dictionary<EffectVisualKeyword, Type>();
			foreach (var visual in effectVisualSubclassTypes)
			{
				var keywordsAtCurrentVisual = visual
					.GetCustomAttributes<EffectVisualAttribute>(false)
					.ToDictionary(a => a.Keyword, _ => visual);
				typeStorage.AddRange(keywordsAtCurrentVisual);
			}
		}

		public bool TryGetType(EffectVisualKeyword value, out Type result)
		{
			return typeStorage.TryGetValue(value, out result);
		}

		public void Dispose()
		{
			typeStorage?.Clear();
			typeStorage = null;
		}
	}
}