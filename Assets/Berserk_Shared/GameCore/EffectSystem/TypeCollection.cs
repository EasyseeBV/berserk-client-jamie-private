using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Berserk.Shared.GameCore.LogicContext;

namespace Berserk.Shared.GameCore.EffectSystem
{
	public class TypeCollection<TKey, TAttribute> where TAttribute : Attribute
	{
		private readonly Dictionary<TKey, Type> cachedTypes;

		public TypeCollection(Func<TAttribute, TKey> selector, Type baseType = null)
		{
			cachedTypes = RegisterTypes(selector, baseType);
		}
		
		private static Dictionary<TKey, Type> RegisterTypes(Func<TAttribute, TKey> selector, Type baseType)
		{
			var effectKeywordSubclassTypes = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => 
				{
					try
					{
						return assembly.GetTypes();
					}
					catch (ReflectionTypeLoadException e)
					{
						DefaultSharedLogger.Error(e);
						return e.Types.Where(type => type != null);
					}
				})
				.Where(x => x.IsClass 
				            && !x.IsAbstract
				            && (baseType == null || x.IsSubclassOf(baseType))
				            && x.CustomAttributes.Any());

			var registred = new Dictionary<TKey, Type>();
			foreach (var type in effectKeywordSubclassTypes)
			{
				foreach (var attribute in type.GetCustomAttributes<TAttribute>(false))
				{
					var key = selector.Invoke(attribute);
					if (registred.ContainsKey(key))
					{
						DefaultSharedLogger.Error($"Key : {key} already exist in collection, type : {type}");
						continue;
					}
					
					registred.Add(selector.Invoke(attribute), type);
				}
			}

			return registred;
		}
		
		public Type Get(TKey key)
		{
			return cachedTypes[key];
		}

		public bool TryGet(TKey key, out Type result)
		{
			return cachedTypes.TryGetValue(key, out result);
		}
	}
}