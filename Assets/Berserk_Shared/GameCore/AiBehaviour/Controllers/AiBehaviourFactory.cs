using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.AiBehaviour.Attributes;
using Berserk.Shared.GameCore.AiBehaviour.NodeExecutors;
using Berserk.Shared.GameCore.LogicContext;

namespace Berserk.Shared.GameCore.AiBehaviour.Controllers
{
	public static class AiBehaviourFactory
	{
		public static readonly Dictionary<AiBehaviourNodeType, Type> CACHED_TYPES;

		static AiBehaviourFactory()
		{
			CACHED_TYPES = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(x => x.IsClass
					&& !x.IsAbstract
					&& x.IsSubclassOf(typeof(AbstractNodeExecutor))
					&& x.CustomAttributes.Any())
				.ToDictionary(
					x => ((AiBehaviourExecutorAttribute)x.GetCustomAttributes(typeof(AiBehaviourExecutorAttribute), true)[0])
						.NodeType,
					y => y);
		}

		public static T GetExecutor<T>(AiBehaviourNodeType nodeType, ISessionProcessor sessionProcessor) where T : AbstractNodeExecutor
		{
			if (!CACHED_TYPES.TryGetValue(nodeType, out var executorType))
				throw new ArgumentNullException($"Executor for {nodeType} not found");

			if (executorType == null)
				throw new ArgumentNullException(
					$"There is no {nameof(nodeType)} class type matching the executor {nodeType}!");

			var instance = (T)Activator.CreateInstance(executorType);
			instance!.Build(sessionProcessor);
			return instance;
		}
	}
}
