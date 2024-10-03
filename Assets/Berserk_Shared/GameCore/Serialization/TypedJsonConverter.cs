using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Berserk.Shared.GameCore.Serialization
{
	public class TypedJsonConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var converters = serializer.Converters.Where(x => x is not TypedJsonConverter).ToArray();

			var jObject = JObject.FromObject(value);
			jObject.AddFirst(new JProperty("$type", value.GetType().Name));
			jObject.WriteTo(writer, converters);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var token = JToken.ReadFrom(reader);
			var obj = (JObject)token;

			var typeProperty = obj.Property("$type");
			if (typeProperty == null)
				throw new ArgumentNullException(nameof(typeProperty));

			var typeName = typeProperty.Value<string>();

			if (string.IsNullOrWhiteSpace(typeName))
				throw new ArgumentNullException(nameof(typeName));

			var type = Type.GetType(typeName);

			if (type == null)
				throw new ArgumentNullException(nameof(type));

			obj.Remove("$type");
			return JsonConvert.DeserializeObject(obj.ToString(), type);
		}

		public override bool CanConvert(Type objectType)
		{
			return true;
		}
	}
}