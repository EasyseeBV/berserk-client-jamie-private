using System;
using System.Collections.Generic;
using System.Linq;

namespace Berserk.Shared.GameCore.Commands
{
    public static class CommandFactory
	{
		private static readonly List<Type> CACHED_TYPES;

		static CommandFactory()
        {
            CACHED_TYPES = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(x => x.IsClass
                            && !x.IsAbstract
                            && x.IsSubclassOf(typeof(Command)))
                .ToList();
        }

		public static T Create<T>(string cmdTypeName, params object[] args) where T : Command
		{
			var cmdType = GetCommandTypeByName(cmdTypeName);

			return cmdType == null
				? throw new ArgumentNullException($"There is no {nameof(Command)} class type matching the command {cmdTypeName}!")
				: (T)Activator.CreateInstance(cmdType, args);
		}

		public static Type GetCommandTypeByName(string cmdTypeName)
		{
			return CACHED_TYPES.FirstOrDefault(x => x.Name == cmdTypeName);
		}

		public static void RegisterCommandType(Type concreteType)
		{
			if (!CACHED_TYPES.Contains(concreteType))
				CACHED_TYPES.Add(concreteType);
		}
    }
}