using System;
using System.Collections.Generic;
using UnityEngine;

namespace RR.Core.Serialization
{
	[Serializable]
	public class UnitySerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
	{
		[SerializeField, HideInInspector]
		private List<TKey> keyData = new List<TKey>();

		[SerializeField, HideInInspector]
		private List<TValue> valueData = new List<TValue>();

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			Clear();
			for (var i = 0; i < keyData.Count && i < valueData.Count; i++) 
				this[keyData[i]] = valueData[i];
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			keyData.Clear();
			valueData.Clear();
			keyData.AddRange(Keys);
			valueData.AddRange(Values);
		}
	}
}
