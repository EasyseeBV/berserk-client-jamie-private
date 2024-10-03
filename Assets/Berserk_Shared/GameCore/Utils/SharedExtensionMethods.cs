using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Lobby;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Attributes;
using Berserk.Shared.GameCore.Commands;

namespace Berserk.Shared.GameCore.Utils
{
	public static class SharedExtensionMethods
	{
		public static bool IsValid(this OwnedCard value)
		{
			return value != null && (value.IsOwned || value.IsSubscription);
		}

		public static bool IsValid(this OwnedVulcanite value)
		{
			return value != null && (value.IsOwned || (value.IsRent && !value.IsExpireRent));
		}

		public static bool IsTableCard(this IRuntimeGameObject value)
		{
			return value is IRuntimeGameCard gameCard && gameCard.Data.Type.IsTableCard();
		}

		public static bool IsTableCard(this ObjectType value)
		{
			return ObjectType.TableCardsMask.HasFlag(value);
		}

		public static bool IsTableEntity(this IRuntimeGameObject value)
		{
			return value is IRuntimeGameCard gameCard && gameCard.Data.Type.IsTableEntity();
		}

		public static bool IsTableEntity(this ObjectType value)
		{
			return ObjectType.TableEntitiesMask.HasFlag(value);
		}

		public static bool IsCard(this IRuntimeGameObject value)
		{
			return value is IRuntimeGameCard gameCard && gameCard.Data.Type.IsCard();
		}

		public static bool IsCard(this ObjectType value)
		{
			return ObjectType.CardsMask.HasFlag(value);
		}

        public static T Map<T>(this T destination, object from)
        {
            if (destination == null)
                throw new NullReferenceException($"{nameof(Map)} cannot executed, {nameof(destination)} '{typeof(T).Name}' object is null");

            if (from == null)
                throw new NullReferenceException($"{nameof(Map)} cannot executed, '{nameof(from)}' object is null");

            var fromFields = from.GetType().GetFields().ToDictionary(x => x.Name);
            var fromProperties = from.GetType().GetProperties().Where(x=> x.CanRead).ToDictionary(x => x.Name);
            
            var destinationFields = destination.GetType().GetFields();
            var destinationProperties = destination.GetType().GetProperties().Where(x=> x.CanWrite).ToArray();
            
            foreach (var destinationField in destinationFields)
            {
                if (fromFields.TryGetValue(destinationField.Name, out var fromFieldInfo)
                    && destinationField.FieldType == fromFieldInfo.FieldType)
                    destinationField.SetValue(destination, fromFieldInfo.GetValue(from));
            }

            foreach (var destinationProperty in destinationProperties)
            {
                if (fromProperties.TryGetValue(destinationProperty.Name, out var fromPropertyInfo)
                    && destinationProperty.PropertyType == fromPropertyInfo.PropertyType)
                    destinationProperty.SetValue(destination, fromPropertyInfo.GetValue(from));
            }

            return destination;
        }

		/// <summary>
		/// Parse from sting e.g : "key1=value1?key2=value2" to Dictionary(string,string).
		/// Where '?' - separate each param.
		/// Where '=' separate key and value.
		/// </summary>
		/// <returns></returns>
		public static Dictionary<string, string> ParseCustomData(this string source)
		{
			if (string.IsNullOrEmpty(source))
				return new Dictionary<string, string>();

			var parameters = source.Split("?", StringSplitOptions.RemoveEmptyEntries);
			if (parameters.Length == 0)
				return new Dictionary<string, string>();

			return parameters.Select(x => x.Split("=", StringSplitOptions.RemoveEmptyEntries))
				.Where(x => x.Length == 2).ToDictionary(key => key[0], value => value[1]);
		}

		/// <summary>
		/// Dictionary(sting, string) to e.g : "key1=value1?key2=value2".
		/// Where '?' - separate each param.
		/// Where '=' separate key and value.
		/// </summary>
		/// <returns></returns>
		public static string ToCustomData(this Dictionary<string, string> source)
		{
			if (source == null || source.Count == 0)
				return string.Empty;

			return source.Where(x => !string.IsNullOrEmpty(x.Key) && !string.IsNullOrEmpty(x.Value))
				.Aggregate("", (c, n) => $"{c}{(string.IsNullOrEmpty(c) ? string.Empty : "?")}{n.Key}={n.Value}");
		}

		/// <summary>
		/// Tuple(key, value) to e.g : "key1=value1?key2=value2".
		/// Where '?' - separate each param.
		/// Where '=' separate key and value.
		/// </summary>
		/// <returns></returns>
		public static string ToCustomData(params (string, object)[] parameters)
		{
			if (parameters == null || parameters.Length == 0)
				return string.Empty;

			return parameters.Where(x => !string.IsNullOrEmpty(x.Item1) && !string.IsNullOrEmpty(x.Item2?.ToString()))
				.Aggregate("", (c, n) => $"{c}{(string.IsNullOrEmpty(c) ? string.Empty : "?")}{n.Item1}={n.Item2}");
		}
		
		public static bool IsCmdLevelAvailable(this CmdExecutionLevel level, string cmd)
		{
			var attribute = CommandFactory.GetCommandTypeByName(cmd)?.GetCustomAttribute<CmdExecutionLevelAttribute>();
			if (attribute == null)
				return false;

			return (attribute.Level & level) != 0;
		}
		
		/// <summary>
		/// If [query items] null or empty return true.
		/// If any [query items] exist in [check list] return true.
		/// If [query items] has except query which contains in [check list] it return false.
		/// Otherwise return false.
		/// </summary>
		/// <param name="query">query items</param>
		/// <param name="exceptFrom">from which item start except part of query</param>
		/// <param name="alwysTrueQueryItem">query an empty item</param>
		/// <param name="checkList">possible items to check</param>
		public static bool AnyExcept<T>(this IReadOnlyCollection<T> query, T exceptFrom, T alwysTrueQueryItem, params T[] checkList)
		{
			if (query == null || query.Count == 0)
				return true;

			checkList ??= Array.Empty<T>();
			
			var result = false;
			var hasExcept = false;
			var comparer = EqualityComparer<T>.Default;
			foreach (var item in query)
			{
				if (comparer.Equals(item, exceptFrom))
				{
					hasExcept = true;
					continue;
				}
				
				if (!hasExcept && result)
					continue;
				
				if (!checkList.Contains(item) && !comparer.Equals(item, alwysTrueQueryItem)) 
					continue;
				
				if (hasExcept)
					return false;
					
				result = true;
			}

			return result;
		}
	}
}