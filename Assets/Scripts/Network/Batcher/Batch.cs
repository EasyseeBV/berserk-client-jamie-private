using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Vulcan.Network
{
	[Serializable]
	[Obsolete("Client not used this model")]
	public class Batch
	{
		public string OwnerUserName { get; } //required by the server to process batch ownership
		public object Data { get; }
		public long Timestamp { get; }
		public MessageType MessageType { get; }

		[JsonConstructor]
		public Batch(object data, MessageType messageType, string ownerUserName)
		{
			Data = data;
			MessageType = messageType;
			OwnerUserName = ownerUserName;
		}
	}

	[JsonConverter(typeof(StringEnumConverter))]
	[Obsolete("Client not used this model")]
	public enum MessageType
	{
		AddCard,
		PlayCard,
		Modify,
		NextRound,
		PerformAction
	}
}