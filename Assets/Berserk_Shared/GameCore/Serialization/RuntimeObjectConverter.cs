using System;
using System.Diagnostics.CodeAnalysis;
using Berserk.Shared.GameCore.Abstraction;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Berserk.Shared.GameCore.Serialization
{
	public class RuntimeObjectConverter : JsonConverter<IRuntimeGameObject>
	{
		public override IRuntimeGameObject ReadJson(JsonReader reader, Type objectType, [AllowNull] IRuntimeGameObject existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			var token = JToken.ReadFrom(reader);
			var obj = (JObject)token;
			var type = Type.GetType(obj.Property("$type").Value.Value<string>());
			obj.Remove("$type");
			return JsonConvert.DeserializeObject(obj.ToString(), type) as IRuntimeGameObject;
		}

		public override void WriteJson(JsonWriter writer, [AllowNull] IRuntimeGameObject value, JsonSerializer serializer)
		{
			var token = JToken.FromObject(value, serializer);
			var obj = (JObject)token;
			obj.AddFirst(new JProperty("$type", value.GetType().FullName));
			obj.WriteTo(writer);
		}
	}
}