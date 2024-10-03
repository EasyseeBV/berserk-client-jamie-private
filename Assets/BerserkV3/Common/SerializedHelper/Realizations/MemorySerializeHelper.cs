using System.Collections.Generic;

namespace BerserkV3.Common.SerializedHelper
{
	public class MemorySerializeHelper : ISerializeHelper
	{
		private readonly Dictionary<string, string> storage;

		public MemorySerializeHelper()
		{
			storage = new Dictionary<string, string>();
		}
		public void Patch(string key, string value)
		{
			if (HasKey(key))
			{
				storage[key] = value;
				return;
			}
			storage.Add(key, value);
		}

		public string Get(string key)
		{
			return HasKey(key) ? storage[key] : "";
		}

		public void Delete(string key)
		{
			if (HasKey(key))
			{
				storage.Remove(key);
			}
		}

		public bool HasKey(string key)
		{
			return storage.ContainsKey(key);
		}

		public void Save(){}
	}
}