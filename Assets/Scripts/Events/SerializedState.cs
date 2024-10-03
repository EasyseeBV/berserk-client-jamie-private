using Newtonsoft.Json;
using RR.Core.EventLayer;
using UnityEngine;

namespace Events
{
	public class SerializedState<T> : State<T> where T : class
	{
		public SerializedState(T value)
		{
			var json = PlayerPrefs.GetString(GetKey(), string.Empty);
			
			if (!string.IsNullOrEmpty(json))
				value = JsonConvert.DeserializeObject<T>(json);

			Publish(value);
		}

		public override void Publish(T data)
		{
			var json = JsonConvert.SerializeObject(data);
			Debug.Log(json);
			PlayerPrefs.SetString(GetKey(), json);
			base.Publish(data);
		}

		private string GetKey()
		{
			return $"SerializedState:{Name}";
		}
	}
}