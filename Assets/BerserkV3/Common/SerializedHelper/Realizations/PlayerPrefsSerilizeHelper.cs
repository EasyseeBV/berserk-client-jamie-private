using UnityEngine;

namespace BerserkV3.Common.SerializedHelper
{
	public class PlayerPrefsSerilizeHelper : ISerializeHelper
	{
		public void Patch(string key, string value)
		{
			PlayerPrefs.SetString(key, value);
		}

		public string Get(string key)
		{
			return PlayerPrefs.GetString(key);
		}

		public void Delete(string key)
		{
			PlayerPrefs.DeleteKey(key);
		}

		public bool HasKey(string key)
		{
			return PlayerPrefs.HasKey(key);
		}

		public void Save()
		{
			PlayerPrefs.Save();
		}
	}
}