using Berserk.Shared.Data.Serialization;
using Newtonsoft.Json;

namespace Berserk.Shared.GameCore
{
	public static class StatsExtensions
	{
		public static T Clone<T>(this T instance)
		{
			if (instance == null)
				return default;
			
			var jsonData = JsonConvert.SerializeObject(instance, SharedSerializationHelper.SerializeSettings);
			return JsonConvert.DeserializeObject<T>(jsonData, SharedSerializationHelper.GetDeserializeSettingsByType<T>());
		}
	}
}