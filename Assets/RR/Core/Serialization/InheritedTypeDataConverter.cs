using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RR.Core.Extensions;

namespace RR.Core.Serialization
{
	/// <summary>
	/// Converts data class inherited from type T.
	/// Derived class must be tagged by <see cref="DataTypeAttribute"/>
	/// Base class must have "DataType" property that is serialized in json.
	/// </summary>
	/// <typeparam name="T">Base type</typeparam>
	public class InheritedTypeDataConverter<T> : JsonConverter where T : class
	{
		public override bool CanWrite => false;
		public override bool CanRead => true;

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(T);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new InvalidOperationException("Use default serialization.");
		}

		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		public override object ReadJson(JsonReader reader,
			Type objectType, object existingValue,
			JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
				return default(T);

			var jsonObject = JObject.Load(reader);
			var missionTypeName = jsonObject["DataType"]?.ToString();

			var inheritedTypes = typeof(T).GetInheritedTypes().Where(x => !x.IsAbstract && !x.IsInterface);
			var missionType = inheritedTypes.FirstOrDefault(type => type.Name == missionTypeName)
							  ?? inheritedTypes.FirstOrDefault();

			if (missionType == null)
				return default(T);

			var missionData = Activator.CreateInstance(missionType);
			serializer.Populate(jsonObject.CreateReader(), missionData);
			return missionData;
		}
	}
}