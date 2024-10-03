using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using RR.Core.DebugSystem;
using RR.Core.Extensions;

namespace BerserkV3.Common.SerializedHelper
{
	public class LocalSerilizeHelper : ISerializeHelper
	{
		private readonly Dictionary<string, string> storage;

		public LocalSerilizeHelper()
		{
			try
			{
				var json = Decrypt(File.ReadAllText(SerilizedHelper.Path));
				storage = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
			}
			catch (Exception e)
			{
				RRLogger.Log($"[{GetType().Name.Orange()}] Can't load save data, Message : {e.Message}");
				storage = new Dictionary<string, string>();
			}
		}

		public void Patch(string key, string value)
		{
			AssertKey(key);
			storage[key] = value;
		}

		public string Get(string key)
		{
			AssertKey(key);
			return storage.TryGetValue(key, out var value) ? value : string.Empty;
		}

		public void Delete(string key)
		{
			AssertKey(key);
			storage.Remove(key);
		}

		public bool HasKey(string key)
		{
			return !string.IsNullOrEmpty(key) && storage.ContainsKey(key);
		}

		public void Save()
		{
			try
			{
				var json = JsonConvert.SerializeObject(storage);
				File.WriteAllText(SerilizedHelper.Path, Crypt(json));
			}
			catch (Exception e)
			{
				RRLogger.Error($"[{GetType().Name.Orange()}] Can't save data, Message : {e.Message}");
			}
		}

		private void AssertKey(string value)
		{
			if (string.IsNullOrEmpty(value))
				throw new ArgumentException($"[{GetType().Name.Orange()}] Key is null or empty");
		}

		private static string Crypt(string text)
		{
			if (string.IsNullOrEmpty(text))
				return string.Empty;
#if UNITY_EDITOR
			return text;
#else
			return Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(text));
#endif
		}

		private static string Decrypt(string text)
		{
			if (string.IsNullOrEmpty(text))
				return string.Empty;
#if UNITY_EDITOR
			return text;
#else
			return System.Text.Encoding.ASCII.GetString(Convert.FromBase64String(text));
#endif
		}
	}
}