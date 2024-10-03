using System;
using Berserk.Shared.Data.Enums;

namespace Vulcan.Data
{
	[Serializable]
	public class EffectState
	{
		//Public fields are used intentionally to implement snapshot mapping when sending a request to the server.
		//There will be a problem with summons when using private properties
		public string Id;

		//remove UID
		public string UID = Guid.NewGuid().ToString();
		public EffectData EffectData; // Do not use for save dynamic - will be replaced
		public string[] Params = Array.Empty<string>();
		public int Length;
		public int Value;

		public EffectState()
		{
		}

		public EffectState(string id, int? length = null, int? value = null, string uid = null, params string[] args)
		{
			// Set(ContentRepository.GetCopyOfEffect(id), length, value, uid, args);
		}

		public EffectState(EffectData data, params string[] args)
		{
			// Set(data, data.Length, data.Value, args: args);
		}

		public EffectState(EffectKeyword effect, int? length = null, int? value = null, params string[] args)
		{
			// Set(ContentRepository.GetCopyOfEffect(effect), length, value, args: args);
		}

		private void Set(EffectData data, int? length, int? value, string uid = null, params string[] args)
		{
			EffectData = data;
			Id = data.Id;
			Length = length ?? EffectData.Length;
			Value = value ?? EffectData.Value;
			Params = args ?? Array.Empty<string>();
			UID = string.IsNullOrEmpty(uid) ? UID : uid;
		}

		public override string ToString()
		{
			return $"[{EffectData.Effect}, UID : {UID}, Id : {Id}, Length : {Length} , Value : {Value}]";
		}
	}
}