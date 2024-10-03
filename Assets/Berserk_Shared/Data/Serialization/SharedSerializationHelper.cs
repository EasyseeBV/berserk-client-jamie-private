using System;
using Newtonsoft.Json;

namespace Berserk.Shared.Data.Serialization
{
	public static class SharedSerializationHelper
	{
		private static ISharedSerializationBinder SerializationBinder { get; } = new SharedSerializationBinder();
		
		public static JsonSerializerSettings DeserializeSettings { get; } = new()
		{
			NullValueHandling = NullValueHandling.Ignore,
			TypeNameHandling = TypeNameHandling.Objects,
			SerializationBinder = SerializationBinder
		};
		
		public static JsonSerializerSettings SerializeSettings { get; } = new()
		{
			TypeNameHandling = TypeNameHandling.Objects
		};

		public static bool IsSupportsWithSerializeSettings(Type type)
		{
			return type?.Assembly?.FullName?.Contains(SerializationBinder.TargetAssemblyName) ?? false;
		}

		public static JsonSerializerSettings GetDeserializeSettingsByType<T>()
		{
			return GetDeserializeSettingsByType(typeof(T));
		}

		public static JsonSerializerSettings GetDeserializeSettingsByType(Type targetType)
		{
			if (targetType.IsGenericParameter)
				targetType = targetType.GetGenericTypeDefinition();
			
			return IsSupportsWithSerializeSettings(targetType) ? DeserializeSettings : default;
		}
	}
}